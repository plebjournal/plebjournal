using Microsoft.FSharp.Core;
using PlebJournal.Dto;

namespace PlebJournal.Templates;

public static class Option
{
    public static bool IsSome<T>(this FSharpOption<T> t) => FSharpOption<T>.get_IsSome(t);
    public static bool IsNone<T>(this FSharpOption<T> t) => FSharpOption<T>.get_IsNone(t);

    public static T? ToNullable<T>(this FSharpOption<T> t) where T : struct =>
        t.IsSome() ? t.Value : null;

    public static T? ToNullableRef<T>(this FSharpOption<T> t) where T : class =>
        t.IsSome() ? t.Value : null;
}

public class OptionVm<T>
{
    private T? _data;
    
    private readonly bool _some = false;

    public bool IsSome() => _some;
    
    public OptionVm(FSharpOption<T> t)
    {
        _some = t.IsSome();
        if (_some)
        {
            var a = t.Value;
        }
        _data = default;
    }

    public static OptionVm<T> OfOption(FSharpOption<T> t) => new OptionVm<T>(t);

    public static implicit operator bool(OptionVm<T> t) => t.IsSome();
}