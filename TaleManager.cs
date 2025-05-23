#region assembly Assembly-CSharp, Version=1.5.9046.17970, Culture=neutral, PublicKeyToken=null
// C:\Users\mfbel\Desktop\RimWorld.v1.5.4241.ALL.DLC\RimWorld.v1.5.4241.ALL.DLC\RimWorld.v1.5.4241.ALL.DLC\RimWorldWin64_Data\Managed\Assembly-CSharp.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public sealed class TaleManager : IExposable
{
    private List<Tale> tales = new List<Tale>();

    private const int MaxUnusedVolatileTales = 350;

    public List<Tale> AllTalesListForReading => tales;

    public void ExposeData()
    {
        Scribe_Collections.Look(ref tales, "tales", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (tales.RemoveAll((Tale x) => x == null) != 0)
            {
                Log.Error("Some tales were null after loading.");
            }

            if (tales.RemoveAll((Tale x) => x.def == null) != 0)
            {
                Log.Error("Some tales had null def after loading.");
            }
        }
    }

    public void TaleManagerTick()
    {
        RemoveExpiredTales();
    }

    public void Add(Tale tale)
    {
        tales.Add(tale);
        CheckCullTales(tale);
    }

    private void RemoveTale(Tale tale)
    {
        if (!tale.Unused)
        {
            Log.Warning("Tried to remove used tale " + tale);
        }
        else
        {
            tales.Remove(tale);
        }
    }

    private void CheckCullTales(Tale addedTale)
    {
        CheckCullUnusedVolatileTales();
        CheckCullUnusedTalesWithMaxPerPawnLimit(addedTale);
    }

    private void CheckCullUnusedVolatileTales()
    {
        int num = 0;
        for (int i = 0; i < tales.Count; i++)
        {
            if (tales[i].def.type == TaleType.Volatile && tales[i].Unused)
            {
                num++;
            }
        }

        while (num > 350)
        {
            Tale tale = null;
            float num2 = float.MaxValue;
            for (int j = 0; j < tales.Count; j++)
            {
                if (tales[j].def.type == TaleType.Volatile && tales[j].Unused && tales[j].InterestLevel < num2)
                {
                    tale = tales[j];
                    num2 = tales[j].InterestLevel;
                }
            }

            RemoveTale(tale);
            num--;
        }
    }

    private void CheckCullUnusedTalesWithMaxPerPawnLimit(Tale addedTale)
    {
        if (addedTale.def.maxPerPawn < 0 || addedTale.DominantPawn == null)
        {
            return;
        }

        int num = 0;
        for (int i = 0; i < tales.Count; i++)
        {
            if (tales[i].Unused && tales[i].def == addedTale.def && tales[i].DominantPawn == addedTale.DominantPawn)
            {
                num++;
            }
        }

        while (num > addedTale.def.maxPerPawn)
        {
            Tale tale = null;
            int num2 = -1;
            for (int j = 0; j < tales.Count; j++)
            {
                if (tales[j].Unused && tales[j].def == addedTale.def && tales[j].DominantPawn == addedTale.DominantPawn && tales[j].AgeTicks > num2)
                {
                    tale = tales[j];
                    num2 = tales[j].AgeTicks;
                }
            }

            RemoveTale(tale);
            num--;
        }
    }

    private void RemoveExpiredTales()
    {
        for (int num = tales.Count - 1; num >= 0; num--)
        {
            if (tales[num].Expired)
            {
                RemoveTale(tales[num]);
            }
        }
    }

    public TaleReference GetRandomTaleReferenceForArt(ArtGenerationContext source)
    {
        if (source == ArtGenerationContext.Outsider)
        {
            return TaleReference.Taleless;
        }

        if (tales.Count == 0)
        {
            return TaleReference.Taleless;
        }

        if (Rand.Value < 0.25f)
        {
            return TaleReference.Taleless;
        }

        if (!tales.Where((Tale x) => x.def.usableForArt).TryRandomElementByWeight((Tale ta) => ta.InterestLevel, out var result))
        {
            return TaleReference.Taleless;
        }

        result.Notify_NewlyUsed();
        return new TaleReference(result);
    }

    public TaleReference GetRandomTaleReferenceForArtConcerning(Thing th)
    {
        if (tales.Count == 0)
        {
            return TaleReference.Taleless;
        }

        if (!tales.Where((Tale x) => x.def.usableForArt && x.Concerns(th)).TryRandomElementByWeight((Tale x) => x.InterestLevel, out var result))
        {
            return TaleReference.Taleless;
        }

        result.Notify_NewlyUsed();
        return new TaleReference(result);
    }

    public Tale GetLatestTale(TaleDef def, Pawn pawn)
    {
        Tale tale = null;
        int num = 0;
        for (int i = 0; i < tales.Count; i++)
        {
            if (tales[i].def == def && tales[i].DominantPawn == pawn && (tale == null || tales[i].AgeTicks < num))
            {
                tale = tales[i];
                num = tales[i].AgeTicks;
            }
        }

        return tale;
    }

    public void Notify_PawnDestroyed(Pawn pawn)
    {
        for (int num = tales.Count - 1; num >= 0; num--)
        {
            if (tales[num].Unused && !tales[num].def.usableForArt && tales[num].def.type != TaleType.PermanentHistorical && tales[num].DominantPawn == pawn)
            {
                RemoveTale(tales[num]);
            }
        }
    }

    public void Notify_PawnDiscarded(Pawn p, bool silentlyRemoveReferences)
    {
        for (int num = tales.Count - 1; num >= 0; num--)
        {
            if (tales[num].Concerns(p))
            {
                if (!silentlyRemoveReferences)
                {
                    Log.Warning(string.Concat("Discarding pawn ", p, ", but he is referenced by a tale ", tales[num], "."));
                }
                else if (!tales[num].Unused)
                {
                    Log.Warning(string.Concat("Discarding pawn ", p, ", but he is referenced by an active tale ", tales[num], "."));
                }

                RemoveTale(tales[num]);
            }
        }
    }

    public void Notify_FactionRemoved(Faction faction)
    {
        for (int i = 0; i < tales.Count; i++)
        {
            tales[i].Notify_FactionRemoved(faction);
        }
    }

    public bool AnyActiveTaleConcerns(Pawn p)
    {
        for (int i = 0; i < tales.Count; i++)
        {
            if (!tales[i].Unused && tales[i].Concerns(p))
            {
                return true;
            }
        }

        return false;
    }

    public bool AnyTaleConcerns(Pawn p)
    {
        for (int i = 0; i < tales.Count; i++)
        {
            if (tales[i].Concerns(p))
            {
                return true;
            }
        }

        return false;
    }

    public float GetMaxHistoricalTaleDay()
    {
        float num = 0f;
        for (int i = 0; i < tales.Count; i++)
        {
            Tale tale = tales[i];
            if (tale.def.type == TaleType.PermanentHistorical)
            {
                float num2 = (float)GenDate.TickAbsToGame(tale.date) / 60000f;
                if (num2 > num)
                {
                    num = num2;
                }
            }
        }

        return num;
    }

    public void LogTales()
    {
        StringBuilder stringBuilder = new StringBuilder();
        IEnumerable<Tale> enumerable = tales.Where((Tale x) => !x.Unused);
        IEnumerable<Tale> enumerable2 = tales.Where((Tale x) => x.def.type == TaleType.Volatile && x.Unused);
        IEnumerable<Tale> enumerable3 = tales.Where((Tale x) => x.def.type == TaleType.PermanentHistorical && x.Unused);
        IEnumerable<Tale> enumerable4 = tales.Where((Tale x) => x.def.type == TaleType.Expirable && x.Unused);
        stringBuilder.AppendLine("All tales count: " + tales.Count);
        stringBuilder.AppendLine("Used count: " + enumerable.Count());
        stringBuilder.AppendLine("Unused volatile count: " + enumerable2.Count() + " (max: " + 350 + ")");
        stringBuilder.AppendLine("Unused permanent count: " + enumerable3.Count());
        stringBuilder.AppendLine("Unused expirable count: " + enumerable4.Count());
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("-------Used-------");
        foreach (Tale item in enumerable)
        {
            stringBuilder.AppendLine(item.ToString());
        }

        stringBuilder.AppendLine("-------Unused volatile-------");
        foreach (Tale item2 in enumerable2)
        {
            stringBuilder.AppendLine(item2.ToString());
        }

        stringBuilder.AppendLine("-------Unused permanent-------");
        foreach (Tale item3 in enumerable3)
        {
            stringBuilder.AppendLine(item3.ToString());
        }

        stringBuilder.AppendLine("-------Unused expirable-------");
        foreach (Tale item4 in enumerable4)
        {
            stringBuilder.AppendLine(item4.ToString());
        }

        Log.Message(stringBuilder.ToString());
    }

    public void LogTaleInterestSummary()
    {
        StringBuilder stringBuilder = new StringBuilder();
        float num = tales.Where((Tale t) => t.def.usableForArt).Sum((Tale t) => t.InterestLevel);
        Func<TaleDef, float> defInterest = (TaleDef def) => tales.Where((Tale t) => t.def == def).Sum((Tale t) => t.InterestLevel);
        foreach (TaleDef def2 in from def in DefDatabase<TaleDef>.AllDefs
                                 where def.usableForArt
                                 orderby defInterest(def) descending
                                 select def)
        {
            stringBuilder.AppendLine(def2.defName + ":   [" + tales.Where((Tale t) => t.def == def2).Count() + "]   " + (defInterest(def2) / num).ToStringPercent("F2"));
        }

        Log.Message(stringBuilder.ToString());
    }
}
#if false // Log de descompilação
'10' itens no cache
------------------
Resolver: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Foi encontrado um assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Carregar de: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.2\mscorlib.dll'
------------------
Resolver: 'UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Foi encontrado um assembly: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Carregar de: 'C:\Users\mfbel\Desktop\RimWorld.v1.5.4241.ALL.DLC\RimWorld.v1.5.4241.ALL.DLC\RimWorld.v1.5.4241.ALL.DLC\RimWorldWin64_Data\Managed\UnityEngine.CoreModule.dll'
------------------
Resolver: 'UnityEngine.TextRenderingModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.TextRenderingModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Foi encontrado um assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Carregar de: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.2\System.Core.dll'
------------------
Resolver: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Foi encontrado um assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Carregar de: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.2\System.dll'
------------------
Resolver: 'NAudio, Version=1.7.3.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'NAudio, Version=1.7.3.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'NVorbis, Version=0.8.4.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'NVorbis, Version=0.8.4.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'com.rlabrecque.steamworks.net, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'com.rlabrecque.steamworks.net, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Foi encontrado um assembly: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Carregar de: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.2\System.Xml.dll'
------------------
Resolver: 'Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Foi encontrado um assembly: 'System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Carregar de: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.2\System.Xml.Linq.dll'
------------------
Resolver: 'Unity.Burst, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'Unity.Burst, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.AssetBundleModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.AssetBundleModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'Unity.TextMeshPro, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'Unity.TextMeshPro, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'ISharpZipLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'ISharpZipLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.InputLegacyModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.InputLegacyModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.PerformanceReportingModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.PerformanceReportingModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.ImageConversionModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.ImageConversionModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.ScreenCaptureModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.ScreenCaptureModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolver: 'UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Não foi possível encontrá-lo por nome: 'UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
#endif
