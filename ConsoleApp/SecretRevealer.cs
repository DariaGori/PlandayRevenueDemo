using Domain.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace ConsoleApp
{
    public class SecretRevealer : ISecretRevealer
    {
        private readonly ApiConfiguration _config;
        public SecretRevealer(IOptions<ApiConfiguration> config)
        {
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        }

        public ApiConfiguration Reveal()
        {
            return _config;
        }
    }
}
