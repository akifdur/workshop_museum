using System;

namespace ClosureSystem
{
    public readonly partial struct Closure
    {
        public static Closure Create(Action action) => new(action);
        public static Closure<TContext> Create<TContext>(Action<TContext> action, TContext context) => new(action, context);
        public static Closure<TContext, TResult> Create<TContext, TResult>(Func<TContext, TResult> action, TContext context) => new(action, context);
    }
    
    public readonly partial struct Closure
    {
        private readonly Action _delegate;

        private Closure(Action del)
        {
            _delegate = del;
        }

        public void Invoke() => _delegate();
    }

    public readonly struct Closure<TContext>
    {
        private readonly Action<TContext> _delegate;
        private readonly TContext _context;
        public TContext Context => _context;

        internal Closure(Action<TContext> del, TContext context)
        {
            _delegate = del;
            _context = context;
        }

        public void Invoke() => _delegate(_context);
    }

    public readonly struct Closure<TContext, TResult>
    {
        private readonly Func<TContext, TResult> _delegate;
        private readonly TContext _context;
        public TContext Context => _context;

        internal Closure(Func<TContext, TResult> del, TContext context)
        {
            _delegate = del;
            _context = context;
        }

        public TResult Invoke() => _delegate(_context);
    }
}