namespace VpkUtils.Utility;

public static class StringExtensions
{
    public static string ToPascalCase(this string str)
    {
        var pascalCaseStr = string.Empty;
        for (var i = 0; i < str.Length; i++)
        {
            if (i == 0) pascalCaseStr += str[i].ToUpperCase();
            else if (str[i].Equals('-') && i + 1 != str.Length)
                pascalCaseStr += str[++i].ToUpperCase();
            else pascalCaseStr += str[i];
        }
        return pascalCaseStr;
    }

    public static string ToKebabCase(this string str)
    {
        var kebabCaseStr = string.Empty;
        for (var i = 0; i < str.Length; i++)
        {
            if (i == 0) kebabCaseStr += str[i].ToLowerCase();
            else if (str[i].IsUpperCase())
                kebabCaseStr += $"-{str[i].ToLowerCase()}";
            else kebabCaseStr += str[i];
        }
        return kebabCaseStr;
    }
}

public static class CharExtensions
{
    public static bool IsLowerCase(this char ch) => ch >= 'a' && ch <= 'z';

    public static bool IsUpperCase(this char ch) => ch >= 'A' && ch <= 'Z';

    public static bool IsAlphabetic(this char ch) =>
        ch.IsLowerCase() || ch.IsUpperCase();

    public static char ToLowerCase(this char ch)
    {
        if (ch.IsLowerCase() || !ch.IsAlphabetic()) return ch;
        else return (char)(ch + 32);
    }

    public static char ToUpperCase(this char ch)
    {
        if (ch.IsUpperCase() || !ch.IsAlphabetic()) return ch;
        else return (char)(ch - 32);
    }
}
