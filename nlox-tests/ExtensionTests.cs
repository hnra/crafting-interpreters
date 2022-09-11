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
}
