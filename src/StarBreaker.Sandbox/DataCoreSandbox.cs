﻿using System.Text;
using System.Xml;
using System.Xml.Linq;
using StarBreaker.Chf;
using StarBreaker.Common;
using StarBreaker.DataCore;
using StarBreaker.P4k;

namespace StarBreaker.Sandbox;

public static class DataCoreSandbox
{
    public static void Run()
    {
        var p4k = P4kFile.FromFile(@"C:\Program Files\Roberts Space Industries\StarCitizen\4.0_PREVIEW\Data.p4k");
        var dcbStream = p4k.OpenRead(@"Data\Game2.dcb");
        var dcb = new DataForge(dcbStream);
        var asdasd = dcb.DataCore.Database.RecordDefinitions.GroupBy(x => x.GetFileName(dcb.DataCore.Database))
            .Where(x => x.Count() > 1)
            .OrderBy(x => x.Count())
            .ToList();

        var keys = asdasd.Select(x => $"{x.Key} - {x.Count()}").ToList();
        var dump = string.Join("\n", keys);

        var xxx = dcb.DataCore.Database.RecordDefinitions.Where(x => x.GetFileName(dcb.DataCore.Database).Contains("TagDatabase.Tagdatabase", StringComparison.InvariantCultureIgnoreCase)).ToList();
        var yyy = xxx.GroupBy(x => x.InstanceIndex);
        var megaMap = dcb.GetRecordsByFileName("*megamap.pu*").Values.Single();
        var tagDatabase = dcb.GetRecordsByFileName("*TagDatabase*").Values.Single();
        var broker = dcb.GetRecordsByFileName("*missionbroker.pu*").Values.Single();
        var unittest = dcb.GetRecordsByFileName("*unittesta*").Values.Single();
        var someActorThing = dcb.DataCore.Database.GetRecord(new CigGuid("41d8fb15-72bb-446e-81df-eaecbc01e195"));

        dcb.GetFromRecord(broker).Save(@"D:\broker.xml");
        dcb.GetFromRecord(unittest).Save(@"D:\unittesta.xml");
        dcb.GetFromRecord(someActorThing).Save(@"D:\someActorThing.xml");
        dcb.GetFromRecord(tagDatabase).Save(@"D:\tagdatabase.xml");
        dcb.GetFromRecord(megaMap).Save(@"D:\megamap.xml");

        dcb.ExtractAllParallel(@"D:\StarCitizen\DataCoreExport");
    }
}