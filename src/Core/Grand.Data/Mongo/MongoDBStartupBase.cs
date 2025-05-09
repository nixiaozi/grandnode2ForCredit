﻿using Grand.Domain.Media;
using Grand.SharedKernel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Grand.Data.Mongo;

public class MongoDBStartupBase : IStartupBase
{
    public int Priority => 0;

    /// <summary>
    ///     Register MongoDB mappings
    /// </summary>
    public void Execute()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));
        BsonSerializer.RegisterSerializer(typeof(Dictionary<int, int>),
            new DictionaryInterfaceImplementerSerializer<Dictionary<int, int>>(DictionaryRepresentation.ArrayOfArrays));

        //global set an equivalent of [BsonIgnoreExtraElements] for every Domain Model
        var cp = new ConventionPack {
            new IgnoreExtraElementsConvention(true),
        };
        ConventionRegistry.Register("ApplicationConventions", cp, t => true);

        BsonClassMap.RegisterClassMap<Download>(cm =>
        {
            cm.AutoMap();
            cm.UnmapMember(m => m.DownloadBinary);
        });
    }
}