using Domain.Configuration;

namespace ConsoleApp
{
    public interface ISecretRevealer
    {
        ApiConfiguration Reveal();
    }
}
