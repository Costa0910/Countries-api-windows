namespace ClassLibraryAPI.Models;

public class Country
{
#region Attributes

    Dictionary<string, double>? _gini;
    List<string>? _capital;
    string? _subregion;
    Dictionary<string, string>? _languages;
    Flags? _flags;

#endregion

#region Properties without deafult values

    public Name Name { get; set; }
    public string Region { get; set; }
    public long Population { get; set; }
    public List<string> Continents { get; set; }

    #endregion

#region Properties with default values
    public string DisplayName { 
        get { 
            return Name.Common;
        } 
    }

    public List<string> Capital
    {
        get
        {
            if (_capital == null)
                _capital = new() { "n\a" };

            return _capital;
        }
        set => _capital = value;
    }

    public string Subregion
    {
        get
        {
            if (_subregion == null)
                return "n\a";

            return _subregion;
        }
        set => _subregion = value;
    }

    public Dictionary<string, double> Gini
    {
        get
        {
            if (_gini == null)
            {
                _gini = _gini = new()
                {
                    ["default"] = 0.0
                };
            }

            return _gini;
        }
        set => _gini = value;
    }

    public Dictionary<string, string> Languages
    {
        get
        {
            if (_languages == null)
            {
                _languages = _languages = new()
                {
                    ["default"] = "n\a"
                };
            }

            return _languages;
        }
        set => _languages = value;
    }

    public Flags Flags
    {
        get
        {
            if (_flags == null)
                _flags = new() { Png = "https://upload.wikimedia.org/wikipedia/commons/6/62/%22No_Image%22_placeholder.png" };

            return _flags;
        }
        set => _flags = value;
    }

#endregion
}