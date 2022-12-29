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
        int AnimationTime(StateKey state);

        /**
         * Checks if the given state is active.
         * 
         * @param state the state key.
         * @return true if the state is set
         */
        bool AnimationState(StateKey state);

        /**
         * Checks if this state was changed based on user interaction or not.
         * If this method returns false then the animation time should not be used
         * for single shot animations.
         *
         * @param state the state key.
         * @return true if single shot animations should run or not.
         */
        bool ShouldAnimateState(StateKey state);
    }

    public class StateKey
    {
        private String _name;
        private int _id;

        private static Dictionary<String, StateKey> KEYS = new Dictionary<String, StateKey>();
        private static List<StateKey> KEYS_BY_ID = new List<StateKey>();

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
            StateKey key = KEYS[name];
            if (key == null)
            {
                key = new StateKey(name, KEYS.Count);
                KEYS.Add(name, key);
                KEYS_BY_ID.Add(key);
            }
            return key;
        }

        public static StateKey Get(int id)
        {
            return KEYS_BY_ID[id];
        }

        public static int StateKeys
        {
            get
            {
                return KEYS.Count;
            }
        }
    }
}
