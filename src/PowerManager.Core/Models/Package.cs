namespace PowerManager.Core.Models;

public class Package
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    
    public bool IsInstalled { get; set; }
    public string InstalledVersion { get; set; } = string.Empty;
    public string AvailableVersion { get; set; } = string.Empty;
    public bool UpdateAvailable { get; set; }
    public DateTime? LastChecked { get; set; }

    public Package() { }
}

