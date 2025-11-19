using SolunarBase.Models;

namespace SolunarBase.Services;

public interface ISolunarCalculator
{
    SolunarResult Calculate(SolunarInput input);
}
