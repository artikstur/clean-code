using MarkdownRenderer;

namespace MarkdownTests;

public class MarkdownConverterTests
{
    private readonly MarkdownConverter _markdownConverter;

    public MarkdownConverterTests()
    {
        var tokensParser = new TokensParser();
        _markdownConverter = new MarkdownConverter(tokensParser);
    }

    [Theory]
    [InlineData("Это __жирный__ текст.", "<div>Это <strong>жирный</strong> текст.</div>")]
    [InlineData("Слова __жирные__ и __жирные__", "<div>Слова <strong>жирные</strong> и <strong>жирные</strong></div>")]
    [InlineData("__Жирный__ в начале.", "<div><strong>Жирный</strong> в начале.</div>")]
    [InlineData("В конце __жирный__", "<div>В конце <strong>жирный</strong></div>")]
    [InlineData("Без __закрытия жирный", "<div>Без __закрытия жирный</div>")]
    [InlineData("Жирный в __нач__але", "<div>Жирный в <strong>нач</strong>але</div>")]
    [InlineData("Жирный в сер__еди__не", "<div>Жирный в сер<strong>еди</strong>не</div>")]
    [InlineData("Жирный в кон__це__", "<div>Жирный в кон<strong>це</strong></div>")]
    [InlineData("Эти __подчерки __считаются__ выделением только во втором слове", "<div>Эти __подчерки <strong>считаются</strong> выделением только во втором слове</div>")]
    [InlineData("Жирный c __циф__ра1ми внутри", "<div>Жирный c __циф__ра1ми внутри</div>")]
    [InlineData("Жирный c __цифра1ми__ снаружи", "<div>Жирный c <strong>цифра1ми</strong> снаружи</div>")]
    [InlineData("Жирный c __цифр__а1ми__ и внутри и снаружи", "<div>Жирный c <strong>цифр__а1ми</strong> и внутри и снаружи</div>")]
    public void ConvertToHtml_ShouldConvertBoldTag(string markdownText, string expectedHtml)
    {
        string result = _markdownConverter.ConvertToHtml(markdownText);

        Assert.Equal(expectedHtml, result);
    }

    [Theory]
    [InlineData("Это _курсивный_ текст.", "<div>Это <em>курсивный</em> текст.</div>")]
    [InlineData("Слова _курсивные_ и _курсивные_", "<div>Слова <em>курсивные</em> и <em>курсивные</em></div>")]
    [InlineData("_Курсивный_ в начале.", "<div><em>Курсивный</em> в начале.</div>")]
    [InlineData("В конце _курсивный_", "<div>В конце <em>курсивный</em></div>")]
    [InlineData("Без _закрытия курсив", "<div>Без _закрытия курсив</div>")]
    [InlineData("Курсив в _нач_але", "<div>Курсив в <em>нач</em>але</div>")]
    [InlineData("Курсив в сер_еди_не", "<div>Курсив в сер<em>еди</em>не</div>")]
    [InlineData("Курсив в кон_це_", "<div>Курсив в кон<em>це</em></div>")]
    [InlineData("Эти _подчерки _считаются_ выделением только во втором слове", "<div>Эти _подчерки <em>считаются</em> выделением только во втором слове</div>")]
    [InlineData("Курсив c _циф_ра1ми внутри", "<div>Курсив c _циф_ра1ми внутри</div>")]
    [InlineData("Курсив c _цифра1ми_ снаружи", "<div>Курсив c <em>цифра1ми</em> снаружи</div>")]
    [InlineData("Курсив c _цифр_а1ми_ и внутри и снаружи", "<div>Курсив c <em>цифр_а1ми</em> и внутри и снаружи</div>")]
    public void ConvertToHtml_ShouldConvertItalicTag(string markdownText, string expectedHtml)
    {
        string result = _markdownConverter.ConvertToHtml(markdownText);

        Assert.Equal(expectedHtml, result);
    }

    [Theory]
    [InlineData("__Внутри двойного выделения _одинарное_ тоже__", "<div><strong>Внутри двойного выделения <em>одинарное</em> тоже</strong></div>")]
    [InlineData("_Внутри одинарного __двойное__ не работает_", "<div><em>Внутри одинарного __двойное__ не работает</em></div>")]
    [InlineData("Эти_ подчерки_ не считаются выделением", "<div>Эти_ подчерки_ не считаются выделением</div>")]
    [InlineData("В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением", "<div>В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением</div>")]
    [InlineData("В то же время выделение в ра_зных сл_овах не работает", "<div>В то же время выделение в ра_зных сл_овах не работает</div>")]
    [InlineData("Проверка пустой строки ____ (она останется)", "<div>Проверка пустой строки ____ (она останется)</div>")]
    [InlineData("Проверка пустой строки __ (она останется)", "<div>Проверка пустой строки __ (она останется)</div>")]
    public void ConvertToHtml_ShouldHandleSpecialCases(string markdownText, string expectedHtml)
    {
        string result = _markdownConverter.ConvertToHtml(markdownText);

        Assert.Equal(expectedHtml, result);
    }

    [Theory]
    [InlineData("# Заголовок", "<div><h1>Заголовок</h1></div>")]
    [InlineData("# Заголовок с __жирным__ текстом", "<div><h1>Заголовок с <strong>жирным</strong> текстом</h1></div>")]
    [InlineData("# Заголовок с _курсивом_", "<div><h1>Заголовок с <em>курсивом</em></h1></div>")]
    [InlineData("# Заголовок с __жирным__ и _курсивом_", "<div><h1>Заголовок с <strong>жирным</strong> и <em>курсивом</em></h1></div>")]
    public void ConvertToHtml_ShouldConvertHeaderTag(string markdownText, string expectedHtml)
    {
        string result = _markdownConverter.ConvertToHtml(markdownText);

        Assert.Equal(expectedHtml, result);
    }

    [Theory]
    [InlineData(@"\_Не курсив в выражении_", @"<div>_Не курсив в выражении_</div>")]
    [InlineData(@"\_НеКурсив_ в слове", @"<div>_НеКурсив_ в слове</div>")]
    [InlineData(@"\__Не жирный в выражении__", @"<div>__Не жирный в выражении__</div>")]
    [InlineData(@"\__НеЖирный__ в слове", @"<div>__НеЖирный__ в слове</div>")]
    [InlineData(@"\\_Курсив в выражении_", @"<div><em>Курсив в выражении</em></div>")]
    [InlineData(@"\\__Жирный в выражении__", @"<div><strong>Жирный в выражении</strong></div>")]
    [InlineData(@"\\_Курсив_ в слове", @"<div><em>Курсив</em> в слове</div>")]
    [InlineData(@"\\__Жирный__ в слове", @"<div><strong>Жирный</strong> в слове</div>")]
    [InlineData(@"Ку\\_рсив_ внутри слова", @"<div>Ку<em>рсив</em> внутри слова</div>")]
    [InlineData(@"Жи\\__рный__ внутри слова", @"<div>Жи<strong>рный</strong> внутри слова</div>")]
    public void ConvertToHtml_ShouldEscapeBackslash(string markdownText, string expectedHtml)
    {
        string result = _markdownConverter.ConvertToHtml(markdownText);

        Assert.Equal(expectedHtml, result);
    }

    [Theory]
    [InlineData(@"Это ссылка на [Google](https://www.google.com)", @"<div>Это ссылка на <a href=""https://www.google.com"">Google</a></div>")]
    [InlineData(@"Это ссылка на [Google](https://www.google.com). с точкой", @"<div>Это ссылка на <a href=""https://www.google.com"">Google</a>. с точкой</div>")]
    [InlineData(@"Это ссылка на .[Google](https://www.google.com). с точкой", @"<div>Это ссылка на .<a href=""https://www.google.com"">Google</a>. с точкой</div>")]
    [InlineData(@"Это ссылка на .[Google](https://www.google.com). с точкой", @"<div>Это ссылка на .<a href=""https://www.google.com"">Google</a>. с точкой</div>")]
    [InlineData("Я люблю [программирование][1] \n [1]: https://www.programming.com/", @"<div>Я люблю <a href=""https://www.programming.com/"" title=>программирование</a></div>")]
    [InlineData("Я люблю текстслева[программирование][1]текстсправа \n [1]: https://www.programming.com/ \"с комментом\"", @"<div>Я люблю текстслева<a href=""https://www.programming.com/"" title=""с комментом"">программирование</a>текстсправа</div>")]
    [InlineData("[1]: https://www.programming.com/ \n Я люблю текстслева[программирование][1]текстсправа ", @"<div>Я люблю текстслева<a href=""https://www.programming.com/"" title=>программирование</a>текстсправа</div>")]
    [InlineData("[1]: https://www.programming.com/ \"с комментом\" \n Я люблю текстслева[программирование][1]текстсправа ", @"<div>Я люблю текстслева<a href=""https://www.programming.com/"" title=""с комментом"">программирование</a>текстсправа</div>")]
    public void ConvertToHtml_ShouldHandleUrl(string markdownText, string expectedHtml)
    {
        string result = _markdownConverter.ConvertToHtml(markdownText);

        Assert.Equal(expectedHtml, result);
    }
}