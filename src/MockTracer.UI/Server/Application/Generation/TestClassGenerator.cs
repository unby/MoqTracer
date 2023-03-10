using Microsoft.Extensions.Options;
using MockTracer.UI.Server.Application.Presentation;
using MockTracer.UI.Shared.Generation;

namespace MockTracer.UI.Server.Application.Generation;

public class TestClassGenerator
{
  private readonly TraceRepository _traceRepository;
  private readonly IBuilderResolver _builderResolver;
  private ClassGenerationSetting _options;

  public TestClassGenerator(IOptions<ClassGenerationSetting> options, TraceRepository traceRepository, IBuilderResolver builderResolver)
  {
    _options = options.Value;
    _traceRepository = traceRepository;
    _builderResolver = builderResolver;
  }

  public async Task<TestFile> CreateAsync(GenerationAttributes @params)
  {
    var data = await _traceRepository.GetGenerationDataAsync(@params);
    var builder = _builderResolver.ResolveTemplateBuilder(@params.TemplateCode);
    var settings = CombineSettings(@params.TestName, _options);
    return new TestFile()
    {
      FileName = settings.DefaultClassName + ".cs",
      SourceCode = builder.Build(
      @params,
      settings,
      data)
    };
  }

  internal static ClassGenerationSetting CombineSettings(string? fullName, ClassGenerationSetting classGenerationSetting)
  {
    if (string.IsNullOrWhiteSpace(fullName))
    {
      return classGenerationSetting;
    }

    var segments = fullName.Split('.');
    return new ClassGenerationSetting()
    {
      DefaultNameSpace = segments.Length > 2 ? string.Join('.', segments.SkipLast(2)) : classGenerationSetting.DefaultNameSpace,
      DefaultClassName = segments.Length > 1 ? segments.SkipLast(1).Last() : classGenerationSetting.DefaultClassName,
      DefaultMethodName = segments.LastOrDefault() ?? classGenerationSetting.DefaultMethodName,
      IsWriteNameSpaceBracket = classGenerationSetting.IsWriteNameSpaceBracket,
      TestBase = classGenerationSetting.TestBase,
    };
  }
}
