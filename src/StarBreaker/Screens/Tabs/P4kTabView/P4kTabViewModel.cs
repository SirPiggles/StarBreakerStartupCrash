using System.Text;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using StarBreaker.Common;
using StarBreaker.Extensions;
using StarBreaker.P4k;
using StarBreaker.Services;

namespace StarBreaker.Screens;

public sealed partial class P4kTabViewModel : PageViewModelBase
{
    public override string Name => "P4k";
    public override string Icon => "ZipFolder";

    private readonly ILogger<P4kTabViewModel> _logger;
    private readonly IP4kService _p4KService;
    
    [ObservableProperty] private HierarchicalTreeDataGridSource<ZipNode> _source;

    [ObservableProperty] private FilePreviewViewModel? _preview;

    private static readonly string[] plaintextExtensions = [".cfg", ".xml", ".txt", ".json"];

    private static readonly string[] ddsLodExtensions = [".dds"];
    //, ".dds.1", ".dds.2", ".dds.3", ".dds.4", ".dds.5", ".dds.6", ".dds.7", ".dds.8", ".dds.9"];

    public P4kTabViewModel(IP4kService p4kService, ILogger<P4kTabViewModel> logger)
    {
        _p4KService = p4kService;
        _logger = logger;
        Source = new HierarchicalTreeDataGridSource<ZipNode>(Array.Empty<ZipNode>())
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ZipNode>(
                    new TextColumn<ZipNode, string>("Name", x => x.GetName(), options: new TextColumnOptions<ZipNode>()
                    {
                        //TODO: make the sorting do folders first, then files
                        //CompareAscending = null,
                        //CompareDescending = null
                    }),
                    x => x.Children.Values
                ),
                new TextColumn<ZipNode, string>("Size", x => x.GetSize(), options: new TextColumnOptions<ZipNode>()
                {
                    CompareAscending = (a, b) => a.ZipEntry?.UncompressedSize.CompareTo(b.ZipEntry?.UncompressedSize) ?? 0,
                    CompareDescending = (a, b) => b.ZipEntry?.UncompressedSize.CompareTo(a.ZipEntry?.UncompressedSize) ?? 0
                }),
                new TextColumn<ZipNode, string>("Date", x => x.GetDate()),
                // new TextColumn<ZipNode, string>("Compression", x => x.CompressionMethodUi),
                // new TextColumn<ZipNode, string>("Encrypted", x => x.EncryptedUi)
            },
        };

        Source.RowSelection!.SingleSelect = true;
        Source.RowSelection.SelectionChanged += SelectionChanged;
        Source.Items = _p4KService.P4kFile.Root.Children.Values;
    }

    private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<ZipNode> e)
    {
        if (e.SelectedItems.Count != 1)
            return;

        //TODO: here, set some boolean that locks the entire UI until the preview is loaded.
        //That is needed to prevent race conditions where the user clicks on another file before the preview is loaded.
        
        var selectedEntry = e.SelectedItems[0];
        if (selectedEntry == null)
        {
            Preview = null;
            return;
        }

        if (selectedEntry.ZipEntry == null)
        {
            //we clicked on a folder, do nothing to the preview.
            Console.WriteLine(selectedEntry.Name);
            return;
        }

        if (selectedEntry.ZipEntry.UncompressedSize > int.MaxValue)
        {
            _logger.LogWarning("File too big to preview");
            return;
        }

        //todo: for a big ass file show a loading screen or something
        Preview = null;
        Task.Run(() =>
        {
            try
            {
                //TODO: move this to a service?
                var buffer = _p4KService.P4kFile.OpenInMemory(selectedEntry.ZipEntry);

                FilePreviewViewModel preview;

                //check cryxml before extension since ".xml" sometimes is cxml sometimes plaintext
                if (CryXmlB.CryXml.TryOpen(new MemoryStream(buffer), out var c))
                {
                    _logger.LogInformation("cryxml");
                    preview = new TextPreviewViewModel(c.ToString());
                }
                else if (plaintextExtensions.Any(p => selectedEntry.GetName().EndsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _logger.LogInformation("plaintextExtensions");

                    preview = new TextPreviewViewModel(Encoding.UTF8.GetString(buffer));
                }
                else if (ddsLodExtensions.Any(p => selectedEntry.GetName().EndsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _logger.LogInformation("ddsLodExtensions");
                    preview = new DdsPreviewViewModel(buffer);
                }
                else
                {
                    _logger.LogInformation("hex");
                    preview = new HexPreviewViewModel(buffer);
                }
                //todo other types

                Dispatcher.UIThread.Post(() => Preview = preview);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to preview file");
            }
        });
    }
}