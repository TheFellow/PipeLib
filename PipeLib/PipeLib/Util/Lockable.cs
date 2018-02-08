using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeLib.Util
{
    /// <summary>
    /// Provides thread safety via locking
    /// </summary>
    /// <typeparam name="T">The Type of the lockable value</typeparam>
    public class Lockable<T>
    {
        private readonly object _lockObject = new object();
        private T _value;

        /// <summary>
        /// The object used internally for lock statements
        /// </summary>
        public object LockObject => _lockObject;

        /// <summary>
        /// <para>
        /// Provides automatic locking during read/writes
        /// </para>
        /// </summary>
        public T Value
        {
            get
            {
                lock (_lockObject)
                {
                    return _value;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _value = value;
                }
            }
        }

        /// <summary>
        /// <para>
        /// To be used by caller, with LockObject, to batch read/writes under one lock)
        /// </para>
        /// </summary>
        public T UnlockedValue
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Executes an action within a lock of the LockObject
        /// </summary>
        /// <param name="action">The action to call within a lock</param>
        public void ExecuteInLock(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            lock (_lockObject)
            {
                action?.Invoke(Value);
            }
        }
        /// <summary>
        /// Executes an action within a lock of the LockObject
        /// </summary>
        /// <param name="func">The function to call within a lock</param>
        public void ExecuteInLock(Func<T, T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            lock (_lockObject)
            {
                Value = func.Invoke(Value);
            }
        }
        /// <summary>
        /// Executes a task within a lock of the LockObject
        /// </summary>
        /// <param name="action">The action to call within a lock</param>
        public async void ExecuteInLockAsync(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    action?.Invoke(Value);
                }
            });
        }
        /// <summary>
        /// Executes a task within a lock of the LockObject
        /// </summary>
        /// <param name="function">The function to call within a lock</param>
        public async void ExecuteInLockAsync(Func<T, T> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    Value = function.Invoke(Value);
                }
            });
        }

        /// <summary>
        /// <para>
        /// Constructor which initializes Value with the default of TValue
        /// </para>
        /// </summary>
        public Lockable() : this(default(T))
        {
        }
        /// <summary>
        /// Constructor which initializes Value with the specified value
        /// </summary>
        /// <param name="value">The initial value for Value</param>
        public Lockable(T value)
        {
            Value = value;
        }
    }
}
