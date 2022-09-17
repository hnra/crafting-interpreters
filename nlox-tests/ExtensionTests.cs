namespace nlox_tests;

public class ExtensionTests
{
    [Test]
    public void CanImportHelloLib()
    {
        var testfile = TestUtilties.GetTestFilePath("hello-lib.lox");
        var lox = new TestLox();
        var source = $@"
import ""./{testfile}"";
hello();
";
        var output = lox.Run(source);
        Assert.IsFalse(lox.hadError);
        Assert.AreEqual(1, output.Count);
        Assert.AreEqual("hello", output[0]);
    }

    [Test]
    public void PreludePow()
    {
        var lox = new TestLox();
        var source = $@"
print pow(4, 2);
print pow(4, 0);
print pow(2, -1);
print pow(4, -2);
";
        var output = lox.Run(source);
        Assert.IsFalse(lox.hadError);
        Assert.AreEqual(4, output.Count);
        Assert.AreEqual($"{Math.Pow(4, 2)}", output[0]);
        Assert.AreEqual($"{Math.Pow(4, 0)}", output[1]);
        Assert.AreEqual($"{Math.Pow(2, -1)}", output[2]);
        Assert.AreEqual($"{Math.Pow(4, -2)}", output[3]);
    }
}
