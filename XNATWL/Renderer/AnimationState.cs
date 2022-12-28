using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface AnimationState
    {

        /**
         * Returns the time since the specified state has changed in ms.
         * If the specified state was never changed then a free running time is returned.
         * 
         * @param state the state key.
         * @return time since last state change is ms.
         */
        int getAnimationTime(StateKey state);

        /**
         * Checks if the given state is active.
         * 
         * @param state the state key.
         * @return true if the state is set
         */
        bool getAnimationState(StateKey state);

        /**
         * Checks if this state was changed based on user interaction or not.
         * If this method returns false then the animation time should not be used
         * for single shot animations.
         *
         * @param state the state key.
         * @return true if single shot animations should run or not.
         */
        bool getShouldAnimateState(StateKey state);
    }

    public class StateKey
    {
        private String _name;
        private int _id;

        private static Dictionary<String, StateKey> keys =
                new Dictionary<String, StateKey>();
        private static List<StateKey> keysByID =
                new List<StateKey>();

        private StateKey(String name, int id)
        {
            this._name = name;
            this._id = id;
        }

        string Name
        {
            get
            {
                return _name;
            }
        }


        int ID
        {
            get
            {
                return _id;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is StateKey) {
                StateKey other = (StateKey)obj;
                return this.ID == other.ID;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public static StateKey Get(String name)
        {
            if (name.Length == 0)
            {
                throw new ArgumentOutOfRangeException("name");
            }
            StateKey key = keys[name];
            if (key == null)
            {
                key = new StateKey(name, keys.Count);
                keys.Add(name, key);
                keysByID.Add(key);
            }
            return key;
        }

        public static StateKey Get(int id)
        {
            return keysByID[id];
        }

        public static int StateKeys
        {
            get
            {
                return keys.Count;
            }
        }
    }
}
