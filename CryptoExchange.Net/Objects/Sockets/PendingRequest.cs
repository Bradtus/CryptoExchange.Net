﻿using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Sockets;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects.Sockets
{
    public abstract class BasePendingRequest
    {
        public int Id { get; set; }
        public Func<BaseParsedMessage, bool> MessageMatchesHandler { get; }
        public bool Completed { get; private set; }
        public abstract Type ResponseType { get; }

        public AsyncResetEvent Event { get; }
        public DateTime RequestTimestamp { get; set; }
        public TimeSpan Timeout { get; }
        public object Request { get; set; }

        private CancellationTokenSource? _cts;
        public abstract CallResult Result { get; set; }

        protected BasePendingRequest(int id, object request, Func<BaseParsedMessage, bool> messageMatchesHandler, TimeSpan timeout)
        {
            Id = id;
            MessageMatchesHandler = messageMatchesHandler;
            Event = new AsyncResetEvent(false, false);
            Timeout = timeout;
            Request = request;
            RequestTimestamp = DateTime.UtcNow;
        }

        public void IsSend()
        {
            // Start timeout countdown
            _cts = new CancellationTokenSource(Timeout);
            _cts.Token.Register(() => Fail("No response"), false);
        }

        public virtual void Fail(string error)
        {
            Completed = true;
            Event.Set();
        }

        public bool MessageMatches(BaseParsedMessage message)
        {
            return MessageMatchesHandler(message);
        }

        public virtual Task ProcessAsync(BaseParsedMessage message)
        {
            Completed = true;
            Event.Set();
            return Task.CompletedTask;
        }
    }

    //public class PendingRequest : BasePendingRequest
    //{
    //    public CallResult Result { get; set; }
    //    public Func<BaseParsedMessage, CallResult> Handler { get; }
    //    public override Type? ResponseType => null;

    //    private PendingRequest(int id, object request, Func<BaseParsedMessage, bool> messageMatchesHandler, Func<BaseParsedMessage, CallResult> messageHandler, TimeSpan timeout)
    //        : base(id, request, messageMatchesHandler, timeout)
    //    {
    //        Handler = messageHandler;
    //    }

    //    public static PendingRequest CreateForQuery(BaseQuery query)
    //    {
    //        return new PendingRequest(ExchangeHelpers.NextId(), query.Request, query.MessageMatchesQuery, query.HandleResult, TimeSpan.FromSeconds(5));
    //    }

    //    public static PendingRequest CreateForSubRequest(Subscription subscription)
    //    {
    //        return new PendingRequest(ExchangeHelpers.NextId(), subscription.GetSubRequest(), subscription.MessageMatchesSubRequest, subscription.HandleSubResponse, TimeSpan.FromSeconds(5));
    //    }

    //    public static PendingRequest CreateForUnsubRequest(Subscription subscription)
    //    {
    //        return new PendingRequest(ExchangeHelpers.NextId(), subscription.GetUnsubRequest(), subscription.MessageMatchesUnsubRequest, subscription.HandleUnsubResponse, TimeSpan.FromSeconds(5));
    //    }

    //    public override void Fail(string error)
    //    {
    //        Result = new CallResult(new ServerError(error));
    //        base.Fail(error);
    //    }

    //    public override Task ProcessAsync(BaseParsedMessage message)
    //    {
    //        Result = Handler(message);
    //        return base.ProcessAsync(message);
    //    }
    //}

    public class PendingRequest<T> : BasePendingRequest
    {
        public override CallResult Result { get; set;  }
        public CallResult<T> TypedResult => (CallResult<T>)Result;
        public Func<ParsedMessage<T>, CallResult<T>> Handler { get; }
        public override Type? ResponseType => typeof(T);

        public PendingRequest(int id, object request, Func<ParsedMessage<T>, bool> messageMatchesHandler, Func<ParsedMessage<T>, CallResult<T>> messageHandler, TimeSpan timeout) 
            : base(id, request, (x) => messageMatchesHandler((ParsedMessage<T>)x), timeout)
        {
            Handler = messageHandler;
        }

        public static PendingRequest<T> CreateForQuery(Query<T> query)
        {
            return new PendingRequest<T>(ExchangeHelpers.NextId(), query.Request, query.MessageMatchesQuery, x =>
            {
                var response = query.HandleResponse(x);
                return response.As((T)response.Data);
            }, TimeSpan.FromSeconds(5));
        }

        public override void Fail(string error)
        {
            Result = new CallResult<T>(new ServerError(error));
            base.Fail(error);
        }

        public override Task ProcessAsync(BaseParsedMessage message)
        {
            Result = Handler((ParsedMessage<T>)message);
            return base.ProcessAsync(message);
        }
    }
}
