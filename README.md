# GamePhysics
An attempt at a more game-y physics system which is more flexible than Unity's default options.

The GamePhysics package contains a more flexable physics system for both 2D and 3D environments. (As of this writing, 2D is almost certainly not functional, but I could get it working again with maybe a few days of development.)

Any actor using the system it broken up into three parts:

 + `GameRigidBody`: Not to be confused with Unity's built-in RigidBody, this component is responsible for the most basic level of movement and collision checks. There are specialized variants to handle various shapes (i.e. Capsule and Box). It allows other components to move either relative to the local rotation, or move as if no rotation is present. It also works with Unity's own arbitrary shape casting (though this is less-than reliable; primitive shapes work better).
 + `Body`: This controls the GameRigidBody's movements. It's responsible for velocity, acceleration, forces, etc. For example, I have a `PlatformerBody` which controls jumping, falling, and walking. In essence, the body component controls what the actor is capable of, but doesn't immediately make any decisions on *what* to do.
 + `Brain`: As its name suggests, this does all the 'thinking' for the actor. In the player's case, it listens for inputs and passes them along to the body. However, the brain could also be AI controlled. And, as a Unity component, nothing stops us from hotswapping brains; I can switch between multiple AI brains, or I could even have multiple brains running at once.

Revisions to this project were initially tracked via Unity Collab, so there are relatively few git commits here.


## Where'll this go from here?

This system is missing the ability to work with certain things in a nice way, such as elevators, slopes, etc. However, these may never get implemented at this point, at least not in this version of the physics system. I never actually got to use this in any game prototypes, but this project has given me insight into how physics systems are built.

I want to investigate Unity's ECS system. I may rebuild this system there, depending on how things turn out.

I also want to investigate Unreal and Godot's physics systems. They may be sufficient for what I need.

If you want to use this but aren't sure what to do (or would like a certain feature), please open up an Issue.


## Why this exists

The GamePhysics package was initially developed for a mixed VR game based on a 2D platforming. I wasn't happy with the default CharacterController, since it was restricted to a capsule. But this left me with two options. The first was to use Unity's dynamic RigidBody physics, but I had a hard time getting this to co-oporate in a way which felt nice to play with. Alternatively, I could abandon Unity's physics systems entirely and build my own.

In retrospect, I probably could have attached a cube to the CharacterController's GameObject, or I could have just directly manipulated the RigidBody's velocities, or maybe a number of other things. But I wanted to get an idea of how this stuff worked.

During the development of the VR game, I discovered a strange bug: If I instantiated the player upside-down, they'd *fall up*. Though I was initially going to patch this out, I realized that it would make an interesting mechanic, so I ended up basing the entire game around this quirk of the physics system.

A few months went by since that VR project was completed, and I found myself repeatedly running into the same problem: I kept having game ideas which would work best as 3rd person platformers, but I didn't have access to a flexible enough physics system. So I began to wonder: Could I take my physics system and make it work in 3D?

So I went to work, some time around the start of 2018. A few months later, and I finally had a decent physics system.

I have never gotten to make it work in any full prototype. By the time it was at a decently functional-state, I had already participated in various game jams, and I decided to focus largely on Omnipresent instead. 
