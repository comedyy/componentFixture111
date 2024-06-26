using System;
using System.Collections.Generic;
using System.IO;

public struct ProfilingScope : IDisposable
{
    CodeGeneratorBuilder codeGeneratorBuilder;
    public ProfilingScope(CodeGeneratorBuilder codeGeneratorBuilder)
    {
        codeGeneratorBuilder._indent++;
        this.codeGeneratorBuilder = codeGeneratorBuilder;
    } 

    public void Dispose()
    {
        codeGeneratorBuilder._indent--;
        codeGeneratorBuilder.AppendLine("}");
    }
}

public class CodeGeneratorBuilder
{
    List<string> _lines = new List<string>();
    string _path;

    private CodeGeneratorBuilder(){}
    public CodeGeneratorBuilder(string path)
    {
        if(string.IsNullOrEmpty(path))
        {
            throw new Exception("path null");
        }

        _path = path;
    }

    public int _indent;

    public void AppendLine(string line)
    {
        for(int i = 0; i < _indent; i++)
        {
            line = "    " + line;
        }

        _lines.Add(line);
    }

    public void NewLine(){
        _lines.Add("");
    }

    // 自动括号。
    public ProfilingScope StartFold(string line)
    {
        AppendLine(line);
        AppendLine("{");

        return new ProfilingScope(this);
    }

    public void Save()
    {
        File.WriteAllLines(_path, _lines.ToArray());
    }
}