﻿using System;
using System.Collections.Generic;

using System.Text;
using System.Threading;

namespace Portfish
{
    internal static class BrokerManager
    {
        internal static void Warmup()
        {
            // Bench 128 4 17 yields:

            // CheckInfoBroker: 140 (4/32/40/32/32)
            // EvalInfoBroker: 16 (4/4/4/4)
            // SwapListBroker: 16 (4/4/4/4)
            // MovesSearchedBroker: 120 (28/36/28/28)
            // PositionBroker: 32 (8/8/8/8)
            // StateInfoArrayBroker: 16 (4/4/4/4)

            // MListBroker: 20 (4/4/4/4/4)
            // LoopStackBroker: 32 (8/8/8/8)
            // MovePickerBroker: 136 (32/40/32/32)
            // StateInfoBroker: 132 (32/36/32/32)

            // Specific allocation not to overallocate memory for nothing
            int i, brokerSize;

            // Reusing brokers
            brokerSize = 40; for (i = 0; i < brokerSize; i++) { CheckInfoBroker.GetObject(); } for (i = 0; i < brokerSize; i++) { CheckInfoBroker.Free(null); }
            brokerSize = 4; for (i = 0; i < brokerSize; i++) { EvalInfoBroker.GetObject(); } for (i = 0; i < brokerSize; i++) { EvalInfoBroker.Free(null); }
            brokerSize = 4; for (i = 0; i < brokerSize; i++) { SwapListBroker.GetObject(); } for (i = 0; i < brokerSize; i++) { SwapListBroker.Free(null); }
            brokerSize = 36; for (i = 0; i < brokerSize; i++) { MovesSearchedBroker.GetObject(); } for (i = 0; i < brokerSize; i++) { MovesSearchedBroker.Free(null); }
            brokerSize = 8; for (i = 0; i < brokerSize; i++) { PositionBroker.GetObject(); } for (i = 0; i < brokerSize; i++) { PositionBroker.Free(null); }
            brokerSize = 4; for (i = 0; i < brokerSize; i++) { StateInfoArrayBroker.GetObject(); } for (i = 0; i < brokerSize; i++) { StateInfoArrayBroker.Free(null); }

            // Recycling brokers
            brokerSize = 4; MList[] arrMList = new MList[brokerSize]; for (i = 0; i < brokerSize; i++) { arrMList[i] = MListBroker.GetObject(); } for (i = brokerSize - 1; i >= 0; i--) { MListBroker.Free(arrMList[i]); }
            brokerSize = 8; LoopStack[] arrLoopStack = new LoopStack[brokerSize]; for (i = 0; i < brokerSize; i++) { arrLoopStack[i] = LoopStackBroker.GetObject(); } for (i = brokerSize - 1; i >= 0; i--) { LoopStackBroker.Free(arrLoopStack[i]); }
            brokerSize = 40; MovePicker[] arrMovePicker = new MovePicker[brokerSize]; for (i = 0; i < brokerSize; i++) { arrMovePicker[i] = MovePickerBroker.GetObject(); } for (i = brokerSize - 1; i >= 0; i--) { MovePickerBroker.Free(arrMovePicker[i]); }
            brokerSize = 36; StateInfo[] arrStateInfo = new StateInfo[brokerSize]; for (i = 0; i < brokerSize; i++) { arrStateInfo[i] = StateInfoBroker.GetObject(); } for (i = brokerSize - 1; i >= 0; i--) { StateInfoBroker.Free(arrStateInfo[i]); }
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(CheckInfoBroker.Report());
            sb.Append(EvalInfoBroker.Report());
            sb.Append(SwapListBroker.Report());
            sb.Append(MovesSearchedBroker.Report());
            sb.Append(PositionBroker.Report());
            sb.Append(StateInfoArrayBroker.Report());

            sb.Append(MListBroker.Report());
            sb.Append(LoopStackBroker.Report());
            sb.Append(MovePickerBroker.Report());
            sb.Append(StateInfoBroker.Report());

            return sb.ToString();
        }
    }

    #region Reusing brokers

    internal static class CheckInfoBroker // Reusing
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly CheckInfo[][] _pool = new CheckInfo[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new CheckInfo[0];
            }
        }

        internal static CheckInfo GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                CheckInfo[] temp = new CheckInfo[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new CheckInfo();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(CheckInfo obj)
        {
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("CheckInfoBroker: {0}{1}\r\n", entryCount, entryCount>0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class EvalInfoBroker // Reusing
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly EvalInfo[][] _pool = new EvalInfo[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new EvalInfo[0];
            }
        }

        internal static EvalInfo GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                EvalInfo[] temp = new EvalInfo[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new EvalInfo();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(EvalInfo obj)
        {
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("EvalInfoBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class SwapListBroker // Reusing
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly SwapList[][] _pool = new SwapList[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new SwapList[0];
            }
        }

        internal static SwapList GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                SwapList[] temp = new SwapList[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new SwapList();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(SwapList obj)
        {
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("SwapListBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class MovesSearchedBroker // Reusing
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly MovesSearched[][] _pool = new MovesSearched[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new MovesSearched[0];
            }
        }

        internal static MovesSearched GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                MovesSearched[] temp = new MovesSearched[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new MovesSearched();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(MovesSearched obj)
        {
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("MovesSearchedBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class StateInfoArrayBroker // Reusing
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly StateInfoArray[][] _pool = new StateInfoArray[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new StateInfoArray[0];
            }
        }

        internal static StateInfoArray GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                StateInfoArray[] temp = new StateInfoArray[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new StateInfoArray();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(StateInfoArray obj)
        {
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("StateInfoArrayBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class PositionBroker // Reusing
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly Position[][] _pool = new Position[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new Position[0];
            }
        }

        internal static Position GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                Position[] temp = new Position[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new Position();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(Position obj)
        {
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("PositionBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    #endregion

    #region Recycling brokers

    internal static class MovePickerBroker // Recycling
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly MovePicker[][] _pool = new MovePicker[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new MovePicker[0];
            }
        }

        internal static MovePicker GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                MovePicker[] temp = new MovePicker[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new MovePicker();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(MovePicker obj)
        {
            obj.Recycle();
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("MovePickerBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class StateInfoBroker // Recycling
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly StateInfo[][] _pool = new StateInfo[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new StateInfo[0];
            }
        }

        internal static StateInfo GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                StateInfo[] temp = new StateInfo[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new StateInfo();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(StateInfo obj)
        {
            obj.Recycle();
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("StateInfoBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class LoopStackBroker // Recycling
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly LoopStack[][] _pool = new LoopStack[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new LoopStack[0];
            }
        }

        internal static LoopStack GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                LoopStack[] temp = new LoopStack[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new LoopStack();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(LoopStack obj)
        {
            obj.Recycle();
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("LoopStackBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    internal static class MListBroker // Recycling
    {
        internal static readonly UInt32[] _cnt = new UInt32[Constants.BROKER_SLOTS];
        internal static readonly MList[][] _pool = new MList[Constants.BROKER_SLOTS][];

        internal static void init()
        {
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                _pool[i] = new MList[0];
            }
        }

        internal static MList GetObject()
        {
            int slotID = System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK;
            if (_cnt[slotID] == _pool[slotID].Length)
            {
                int poolLength = _pool[slotID].Length;
                MList[] temp = new MList[poolLength + Constants.BrokerCapacity];
                Array.Copy(_pool[slotID], temp, poolLength);
                for (int i = 0; i < Constants.BrokerCapacity; i++)
                {
                    temp[poolLength + i] = new MList();
                }
                _pool[slotID] = temp;
            }
            return _pool[slotID][_cnt[slotID]++];
        }

        internal static void Free(MList obj)
        {
            obj.Recycle();
            _cnt[System.Threading.Thread.CurrentThread.ManagedThreadId & Constants.BROKER_SLOT_MASK]--;
        }

        internal static string Report()
        {
            StringBuilder sb = new StringBuilder();
            int entryCount = 0;
            for (int i = 0; i < Constants.BROKER_SLOTS; i++)
            {
                if (_pool[i].Length > 0) { entryCount += _pool[i].Length; sb.Append("/").Append(_pool[i].Length); }
            }
            return string.Format("MListBroker: {0}{1}\r\n", entryCount, entryCount > 0 ? string.Format(" ({0})", sb.ToString().Substring(1)) : string.Empty);
        }
    }

    #endregion
}
