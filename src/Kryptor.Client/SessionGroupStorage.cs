using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    public partial class SessionGroup
    {
        object _addLock = new object();

        readonly HashSet<SessionHolder> storage = new HashSet<SessionHolder>();

        /// <summary>
        /// Gets a bool indicates the lock status of this session group.
        /// </summary>
        /// <remarks>
        /// Whenever the first session has been started, the session group will be locked.
        /// </remarks>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// Throws a new <see cref="InvalidOperationException"/> when the session group is locked.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void ThrowIfLocked()
        {
            if (IsLocked)
            {
                throw new InvalidOperationException("The session group is locked");
            }
        }

        /// <inheritdoc/>
        public int Count => storage.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => IsLocked;

        /// <inheritdoc/>
        public void Add(SessionHolder item)
        {
            ThrowIfLocked();

            lock (_addLock)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));

                if (!item.Session.IsReady(CancellationToken.None))
                {
                    throw new NotSupportedException("Session dependency feature is not supported by SessionGroup");
                }

                if (item.Session.Status != SessionStatus.NotStarted)
                {
                    throw new ArgumentException("Cannot add an already started session");
                }

                if (storage.Add(item))
                {
                    AddHooks(item);
                }
                else
                {
                    throw new InvalidOperationException("Item is already exists");
                }
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ThrowIfLocked();
            storage.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(SessionHolder item) => storage.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(SessionHolder[] array, int arrayIndex) => storage.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public bool Remove(SessionHolder item)
        {
            ThrowIfLocked();
            return storage.Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<SessionHolder> GetEnumerator() => storage.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => storage.GetEnumerator();
    }
}
