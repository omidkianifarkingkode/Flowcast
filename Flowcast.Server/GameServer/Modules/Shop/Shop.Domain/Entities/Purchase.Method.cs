using Shop.Domain.Enums;

namespace Shop.Domain.Entities
{
    public sealed partial class Purchase
    {
        private Purchase() { }
        private Purchase(
            PurchaseId id,
            OrderId orderId,
            Store store,
            PurchaseToken purchaseToken,
            PurchaseSignature? signature,
            string productId,
            string receipt,
            string payload,
            DateTimeOffset purchaseAtUtc,
            string userId,
            bool isSandbox,
            PurchaseState state,
            DateTimeOffset CreatedAtUtc
            ) : base(id)
        {

            Id = id;
            OrderId = orderId;
            Store = store;
            PurchaseToken = purchaseToken;
            Signature = signature;
            ProductId = productId;
            Receipt = receipt;
            Payload = payload;
            PurchaseAtUtc = purchaseAtUtc;
            UserId = userId;
            _isSandBox = isSandbox;
            State = state;
            CreatedAtUtc = DateTimeOffset.UtcNow;

        }
        public static Purchase LogNew(
            PurchaseId id,
            OrderId orderId,
            Store store,
            PurchaseToken token,
            string productId,
            string receipt,
            string payload,
            string userId,
            DateTimeOffset purchaseAtUtc,
            bool isSandbox = false,
            PurchaseSignature? signature = null,
            Dictionary<string, object>? meta = null
            )
        {
            if (string.IsNullOrWhiteSpace(id.Value))
                throw new ArgumentException("Purchase Id Cannot be empty.");
            if (orderId == null)
                throw new ArgumentNullException(nameof(orderId));
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("ProductId cannot be empty");
            if (string.IsNullOrWhiteSpace(receipt))
                throw new ArgumentException("Receipt cannot be empty");
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Payload cannot be empty");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be empty");

            return new Purchase
            {
                Id = id,
                OrderId = orderId,
                Store = store,
                PurchaseToken = token,
                ProductId = productId,
                Receipt = receipt,
                Payload = payload,
                UserId = userId,
                PurchaseAtUtc = purchaseAtUtc,
                _isSandBox = isSandbox,
                Signature = signature,
                State = PurchaseState.LOGGED,
                Meta = meta ?? new Dictionary<string, object>(),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
        }
        public void MarkValidating(DateTimeOffset now)
        {
            State = PurchaseState.VALIDATING;
            UpdatedAtUtc = now;
        }

        /// <summary>
        /// Both method are only written for suggestion
        /// </summary>
        /// <param name="now"></param>
        /// <exception cref="ArgumentException"></exception>
        #region Suggestion method
        public void MarkValid(DateTimeOffset now)
        {
            if(State is PurchaseState.VALID or PurchaseState.INVALID)
                throw new ArgumentException("Purchase already finalized");
            State = PurchaseState.VALID;
            UpdatedAtUtc = now;
        }
        public void MarkInvalid(string reason, DateTimeOffset now)
        {
            if(State is PurchaseState.VALID or PurchaseState.INVALID)
                throw new ArgumentException("Purchase already finalized");
            Meta["invalid_reason"] = reason;
            State = PurchaseState.INVALID;
            UpdatedAtUtc = now;
        }
        #endregion
        public void SetState(PurchaseState newState, DateTimeOffset now)
        {
            State = newState;
            UpdatedAtUtc = now;
        }
        public void ResetToLogged(DateTimeOffset now)
        {
            State = PurchaseState.LOGGED;
            UpdatedAtUtc = now;
        }

        public void AddValidationAttempt(PurchaseValidationAttempt attempt)
        {
            if (attempt == null) throw new ArgumentNullException(nameof(attempt));
            _validationAttempts.Add(attempt);
        }

    }
}
