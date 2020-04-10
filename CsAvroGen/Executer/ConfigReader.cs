﻿using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using CsAvroGen.DomainModel;

namespace holonsoft.CsAvroGen.Executer
{
    internal class ConfigReader
    {
        internal void Read(TypeInfoData typeInfoData)
        {
            typeInfoData.IndentFactor = 2;

            var configFileName = Assembly.GetEntryAssembly().Location + ".config";

            if (!File.Exists(configFileName)) return;

            Console.WriteLine("Reading config from " + configFileName);

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (key.StartsWith("MetadataReference::Load", StringComparison.InvariantCultureIgnoreCase))
                {
                    var v = ConfigurationManager.AppSettings.Get(key);

                    if (string.IsNullOrWhiteSpace(v)) continue;

                    typeInfoData.MetadataReferenceList.Add(MetadataReference.CreateFromFile(v));
                }

                if (string.Compare(key, "IndentFactor", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    var v = ConfigurationManager.AppSettings.Get(key);

                    if (int.TryParse(v, out int result))
                    {
                        typeInfoData.IndentFactor = result;
                    }
                    else
                    {
                        typeInfoData.IndentFactor = 2;
                    }
                }
            }
        }
    }
}