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

    [Test]
    public void CanDefineList()
    {
        var lox = new TestLox();
        var source = $@"
var myList = [1, ""apa"", pow(2, 2), nil];
print myList.length();
print myList.at(0);
print myList.at(1);
print myList.at(2);
print myList.at(3);
";
        var output = lox.Run(source);
        Assert.IsFalse(lox.hadError);
        Assert.AreEqual(5, output.Count);
        Assert.AreEqual("4", output[0]);
        Assert.AreEqual("1", output[1]);
        Assert.AreEqual("apa", output[2]);
        Assert.AreEqual("4", output[3]);
        Assert.AreEqual("nil", output[4]);
    }

    [Test]
    public void IndexOutOfRangeIsError()
    {
        var lox = new TestLox();
        var source = $@"
var myList = [1];
myList.at(1);
";
        var output = lox.Run(source);
        Assert.IsTrue(lox.hadError);
    }

    [Test]
    public void NegativeIndexAccessesFromTheBack()
    {
        var lox = new TestLox();
        var source = $@"
var myList = [1, 2];
print myList.at(-1);
print myList.at(-2);
";
        var output = lox.Run(source);
        Assert.IsFalse(lox.hadError);
        Assert.AreEqual(2, output.Count);
        Assert.AreEqual("2", output[0]);
        Assert.AreEqual("1", output[1]);
    }

    [Test]
    public void VectorsCanContainExpressions()
    {
        var lox = new TestLox();
        var source = $@"
var myList = [1 + 1, !true, true ? -1 : 10];
print myList.at(0);
print myList.at(1);
print myList.at(2);
";
        var output = lox.Run(source);
        Assert.IsFalse(lox.hadError);
        Assert.AreEqual(3, output.Count);
        Assert.AreEqual("2", output[0]);
        Assert.AreEqual("false", output[1]);
        Assert.AreEqual("-1", output[2]);
    }

    [Test]
    public void VectorsCanContainVectors()
    {
        var lox = new TestLox();
        var source = $@"
var myList = [[1,2,3]];
print myList.at(0).at(0);
print myList.at(0).at(1);
print myList.at(0).at(2);
";
        var output = lox.Run(source);
        Assert.IsFalse(lox.hadError);
        Assert.AreEqual(3, output.Count);
        Assert.AreEqual("1", output[0]);
        Assert.AreEqual("2", output[1]);
        Assert.AreEqual("3", output[2]);
    }
}
