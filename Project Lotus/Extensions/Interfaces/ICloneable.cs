namespace Lotus.Extensions.Interfaces;

public interface ICloneable<out T>
{
    T Clone();
}