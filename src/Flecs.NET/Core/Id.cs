using System;
using Flecs.NET.Utilities;
using static Flecs.NET.Bindings.Native;

namespace Flecs.NET.Core
{
    /// <summary>
    ///     Class that wraps around a flecs::id_t.
    /// </summary>
    public unsafe struct Id : IEquatable<Id>
    {
        private ecs_world_t* _world;
        private ulong _value;

        /// <summary>
        ///     A reference to the world.
        /// </summary>
        public ref ecs_world_t* World => ref _world;

        /// <summary>
        ///     A reference to the id value.
        /// </summary>
        public ref ulong Value => ref _value;

        /// <summary>
        ///     Creates an id with the provided id value.
        /// </summary>
        /// <param name="id"></param>
        public Id(ulong id)
        {
            _world = null;
            _value = id;
        }

        /// <summary>
        ///     Creates an id with the provided pair.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public Id(ulong first, ulong second)
        {
            _world = null;
            _value = Macros.Pair(first, second);
        }

        /// <summary>
        ///     Creates an id with the provided world and pair.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public Id(ecs_world_t* world, ulong first, ulong second)
        {
            _world = world;
            _value = Macros.Pair(first, second);
        }

        /// <summary>
        ///     Creates an id with the provided world and id value.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="id"></param>
        public Id(ecs_world_t* world, ulong id = 0)
        {
            _world = world;
            _value = id;
        }

        /// <summary>
        ///     Creates an id with the provided pair.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public Id(Id first, Id second)
        {
            _world = first.World;
            _value = Macros.Pair(first.Value, second.Value);
        }

        /// <summary>
        ///     Creates an id with the provided pair.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public Id(Entity first, Entity second)
        {
            _world = first.World;
            _value = Macros.Pair(first, second);
        }

        /// <summary>
        ///     Test if id is pair. (has first, second)
        /// </summary>
        /// <returns></returns>
        public bool IsPair()
        {
            return (Value & ECS_ID_FLAGS_MASK) == ECS_PAIR;
        }

        /// <summary>
        ///     Test if id is a wildcard.
        /// </summary>
        /// <returns></returns>
        public bool IsWildCard()
        {
            return ecs_id_is_wildcard(Value) == 1;
        }

        /// <summary>
        ///     Test if id is an entity.
        /// </summary>
        /// <returns></returns>
        public bool IsEntity()
        {
            return (Value & ECS_ID_FLAGS_MASK) == 0;
        }

        /// <summary>
        ///     Return id as entity. (only allowed when id is valid entity)
        /// </summary>
        /// <returns></returns>
        public Entity Entity()
        {
            Assert.True(!IsPair());
            Assert.True(Flags() == 0);
            return new Entity(World, Value);
        }

        /// <summary>
        ///     Return id with role added
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Entity AddFlags(ulong flags)
        {
            return new Entity(World, Value | flags);
        }

        /// <summary>
        ///     Return id with role removed.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Entity RemoveFlags(ulong flags)
        {
            Assert.True((Value & ECS_ID_FLAGS_MASK) == flags);
            return new Entity(World, Value & ECS_COMPONENT_MASK);
        }

        /// <summary>
        ///     Return id without role.
        /// </summary>
        /// <returns></returns>
        public Entity RemoveFlags()
        {
            return new Entity(World, Value & ECS_COMPONENT_MASK);
        }

        /// <summary>
        ///     Return id without generation.
        /// </summary>
        /// <returns></returns>
        public Entity RemoveGeneration()
        {
            return new Entity(World, (uint)Value);
        }

        /// <summary>
        ///     Return component type of id.
        /// </summary>
        /// <returns></returns>
        public Entity TypeId()
        {
            return new Entity(World, ecs_get_typeid(World, Value));
        }

        /// <summary>
        ///     Test if id has specified flags.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HasFlags(ulong flags)
        {
            return (Value & flags) == flags;
        }

        /// <summary>
        ///     Test if id has flags.
        /// </summary>
        /// <returns></returns>
        public bool HasFlags()
        {
            return (Value & ECS_ID_FLAGS_MASK) != 0;
        }

        /// <summary>
        ///     Return id flags set on id.
        /// </summary>
        /// <returns></returns>
        public Entity Flags()
        {
            return new Entity(World, Value & ECS_ID_FLAGS_MASK);
        }

        /// <summary>
        ///     Test if id has specified first.
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
        public bool HasRelation(ulong first)
        {
            return IsPair() && Macros.PairFirst(Value) == first;
        }

        /// <summary>
        ///     Get first element from a pair.
        ///     If the id is not a pair, this operation will fail. When the id has a
        ///     world, the operation will ensure that the returned id has the correct
        ///     generation count.
        /// </summary>
        /// <returns></returns>
        public Entity First()
        {
            Assert.True(IsPair());
            ulong entity = Macros.PairFirst(Value);
            return World == null ? new Entity(entity) : new Entity(World, ecs_get_alive(World, entity));
        }

        /// <summary>
        ///     Get second element from a pair.
        ///     If the id is not a pair, this operation will fail. When the id has a
        ///     world, the operation will ensure that the returned id has the correct
        ///     generation count.
        /// </summary>
        /// <returns></returns>
        public Entity Second()
        {
            ulong entity = Macros.PairSecond(Value);
            return World == null ? new Entity(entity) : new Entity(World, ecs_get_alive(World, entity));
        }

        /// <summary>
        ///     Convert id to string.
        /// </summary>
        /// <returns></returns>
        public string Str()
        {
            return NativeString.GetStringAndFree(ecs_id_str(World, Value));
        }

        /// <summary>
        ///     Convert role of id to string.
        /// </summary>
        /// <returns></returns>
        public string FlagsStr()
        {
            return NativeString.GetStringAndFree(ecs_id_flag_str(Value & ECS_ID_FLAGS_MASK));
        }

        /// <summary>
        ///     Returns the C# world.
        /// </summary>
        /// <returns></returns>
        public World CsWorld()
        {
            return new World(World);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator ulong(Id id)
        {
            return id.Value;
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static implicit operator Id(ulong id)
        {
            return new Id(id);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Id left, Id right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Id left, Id right)
        {
            return !(left == right);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Id FromUInt64(ulong id)
        {
            return new Id(id);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ulong ToUInt64(Id id)
        {
            return id.Value;
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Id other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is Id id && Equals(id);
        }

        /// <summary>
        ///     Returns a hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        ///     Returns the string representation of the Id.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Str();
        }
    }
}
