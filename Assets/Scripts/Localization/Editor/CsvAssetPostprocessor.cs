using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Localization.Editor
{
    class CsvAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (string str in importedAssets)
            {
                if (Path.GetExtension(str).Equals(".csv", System.StringComparison.OrdinalIgnoreCase))
                {
                    OnCsvImported(str);
                }
            }
        }

        static void OnCsvImported(string path)
        {
            var guids = AssetDatabase.FindAssets("t:Table");
            foreach (var guid in guids)
            {
                var tablePath = AssetDatabase.GUIDToAssetPath(guid);
                var table = AssetDatabase.LoadAssetAtPath<Table>(tablePath);

                if (table.csvFile == null)
                {
                    continue;
                }

                var tableCsvPath = AssetDatabase.GetAssetPath(table.csvFile);
                if (tableCsvPath == path)
                {
                    table.ReadCsv();
                }
            }
        }
    }
}