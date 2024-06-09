using Org.BouncyCastle.Asn1.CryptoPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PlayerGuesser
{
    internal class TokenBucket
    {
        private int Capacity {  get; set; }
        internal DateTime RefillTime { get; set; }
        private int RefillAmount { get; set; }
        private int CurrentTokens { get; set; }

        public TokenBucket CreateBucket(int capacity, int refillAmount)
        {
            Capacity = capacity;
            RefillAmount = refillAmount;
            CurrentTokens = capacity;
            RefillTime = DateTime.UtcNow;
            return this;
        }
        public async Task UpdateBucket(TokenBucket bucket)
        {
            DateTime currentTime = DateTime.UtcNow;
            TimeSpan elapsedTime = currentTime - bucket.RefillTime;

            //kept 60 seconds because both are /minute
            if (elapsedTime.TotalSeconds >= 60)
            {
                CurrentTokens = Math.Min(Capacity, CurrentTokens + RefillAmount);
                RefillTime = DateTime.UtcNow;
            }
            else
            {
                var timeLeft = Math.Ceiling(60 - elapsedTime.TotalSeconds);
                int timeConverted = (int)timeLeft;
                await Task.Delay(timeConverted * 1000);
                CurrentTokens = Math.Min(Capacity, CurrentTokens + RefillAmount);
                RefillTime = DateTime.UtcNow;
            }
        }
        public async Task HandleRequest(TokenBucket bucket)
        {
            if (bucket.CurrentTokens <= bucket.Capacity / 1.5)
            {
                await UpdateBucket(bucket);
            }
            //if sharedbucket
            if (bucket.Capacity == 100)
            {
                if (bucket.CurrentTokens > 3)
                {
                    bucket.CurrentTokens -= 3;
                }
                else
                {
                    await UpdateBucket(bucket);
                    if (bucket.CurrentTokens > 3)
                    {
                        bucket.CurrentTokens -= 3;
                    }
                }
            }
            else
            {
                if (bucket.CurrentTokens > 0)
                {
                    bucket.CurrentTokens--;
                }
                else
                {
                    await UpdateBucket(bucket);
                    if (bucket.CurrentTokens > 0)
                    {
                        bucket.CurrentTokens--;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to handle request; token bucket is empty.");
                    }
                }
            }
        }

    }    
}
