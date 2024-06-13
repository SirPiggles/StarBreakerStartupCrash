﻿using System.Text.RegularExpressions;

namespace StarBreaker.Forge;

public interface IDataForge
{
    /// <summary>
    ///     Export all files in the DataForge to XML.
    /// </summary>
    /// <param name="fileNameFilter">Regex to filter files to export</param>
    /// <param name="progress">Progress callback</param>
    void Export(Regex? fileNameFilter = null, IProgress<float>? progress = null);
    
    /// <summary>
    ///     Export all records in the DataCoreBinary into a single XML file.
    /// </summary>
    /// <param name="fileNameFilter">Regex to filter files to export</param>
    /// <param name="progress">Progress callback</param>
    void ExportSingle(Regex? fileNameFilter = null, IProgress<float>? progress = null);
    
    /// <summary>
    ///     Export all enums in the DataForge to a dictionary.
    /// </summary>
    Dictionary<string, string[]> ExportEnums();
}