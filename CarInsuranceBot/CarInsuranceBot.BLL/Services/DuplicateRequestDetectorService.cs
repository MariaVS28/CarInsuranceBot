using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace CarInsuranceBot.BLL.Services
{
    public class DuplicateRequestDetectorService() : IDuplicateRequestDetectorService
    {
        private readonly ConcurrentDictionary<(long userId, string hash), DateTime> _messageCache = new();
        private readonly TimeSpan Expiration = TimeSpan.FromSeconds(5);

        public bool IsDuplicate(long telegramUserId, string message)
        {
            var hash = ComputeHash(message);
            var key = (telegramUserId, hash);

            var now = DateTime.UtcNow;

            if (_messageCache.TryGetValue(key, out var timestamp))
            {
                if (now - timestamp < Expiration)
                    return true;
            }

            _messageCache[key] = now;

            if (_messageCache.Count > 100)
            {
                CleanupOldEntries(now);
            }
            

            return false;
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes);
        }

        private void CleanupOldEntries(DateTime now)
        {
            foreach (var entry in _messageCache)
            {
                if (now - entry.Value > Expiration)
                {
                    _messageCache.TryRemove(entry.Key, out _);
                }
            }
        }
    }
}
