#region assembly Assembly-CSharp, Version=1.5.9046.17970, Culture=neutral, PublicKeyToken=null
// C:\Users\mfbel\Desktop\RimWorld.v1.5.4241.ALL.DLC\RimWorld.v1.5.4241.ALL.DLC\RimWorld.v1.5.4241.ALL.DLC\RimWorldWin64_Data\Managed\Assembly-CSharp.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class RelationshipRecords : IExposable
{
    private Dictionary<int, RelationshipRecord> records = new Dictionary<int, RelationshipRecord>();

    private static readonly HashSet<int> toRemove = new HashSet<int>();

    private List<int> tmpKeys;

    private List<RelationshipRecord> tmpValues;

    public IReadOnlyDictionary<int, RelationshipRecord> Records => records;

    public RelationshipRecord GetOrCreateRecord(Pawn pawn)
    {
        if (records.TryGetValue(pawn.thingIDNumber, out var value))
        {
            return value;
        }

        return CreateRecord(pawn);
    }

    public RelationshipRecord CreateRecord(Pawn pawn)
    {
        RelationshipRecord relationshipRecord = new RelationshipRecord(pawn.thingIDNumber, pawn.gender, pawn.Name.ToStringShort);
        records.Add(relationshipRecord.ID, relationshipRecord);
        return relationshipRecord;
    }

    public RelationshipRecord GetRecord(int id)
    {
        return records[id];
    }

    public int CleanupUnusedRecords()
    {
        toRemove.Clear();
        foreach (KeyValuePair<int, RelationshipRecord> record in records)
        {
            if (record.Value.References.Count <= 1)
            {
                toRemove.Add(record.Key);
            }
        }

        foreach (int item in toRemove)
        {
            records.Remove(item);
        }

        int count = toRemove.Count;
        toRemove.Clear();
        return count;
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref records, "records", LookMode.Value, LookMode.Deep, ref tmpKeys, ref tmpValues);
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
