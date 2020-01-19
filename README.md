# Superball-Destruction

<h1>The goal of this game</h1>

This was an attempt to make a game out of the scene in Men in Black where J touches a yellow orb, which immediately destroys everything inside MIB headquarters. The player aims a cannon and fires it at ceramic plates in a small room. If the ball hits a plate, the plate will "disappear" and its velocity will increase. If it hits anything else in the room - in this game, a wall or a shelf - the ball's velocity will decrease. Once the superball's velocity reaches a value less than 5mph, gravity will be reactivated, and the game ends when the y-component of the ball's velocity is <= 0.0f. Your score is the number of breakable objects you hit x 100.

The aesthetic is simple: breaking imaginary things in a room is fun and hilarious (unlike real things, of which the player would be financially liable). Also,  the physics of a ball rebounding off a solid surface (or breaking a solid surface) lends itself to a system for the player to practice and create high scores. Furthermore, when the player's shot ends up doing some astronomically catastrophic damage - such as when the ball hits the chemistry set in MIB - the outcomes could be hilarious to the player.

<h1>The Results</h1>

<h2>Programming</h2>

I also wanted to make this game to improve my skills in Unity3D by focusing on systems I had not used before. I wrote a shader and combined it with a texture of repeating-arrows to create a material that displays the ball's trajectory when it's shot out of a cannon. Then I used Quaternions to calculate the ball's expected trajectory in this "Aiming state" to create another texture that pointed in the direction the ball would bounce when it collided with an object the cannon was aimed at, deriving a Vector and orientation to apply to the second texture.

Next, I used Unity3D's physics system to rewrite the rules governing the ball's "bounciness": to determine if an object is Breakable, the GameObject the ball collides with is checked in Physics.OnCollisionEnter and OnCollisionExit, and I was also able to adjust the ball's velocity, creating its own rules of physics, which met the needs for my designs. I learned how to use Unity's EventManagement system (which made scripting much easier), and I learned how to use Quaternions.

Lastly, I taught myself how to use Unity3D's Event Messaging System. The class was developed to resemble the Observer Design Pattern: the programmer creates Interfaces derived from IEventSystemHandler, and uses a static class EventSystems to send messages and lambda functions to any GameObject containing a script that implements your interfaces (derived from IEventSystemHandler).

I discuss some additional features I developed in the following section on Design and Playability.

<h2>Design and Playability</h2>

I need to discuss the game's shortcomings. Simply put, it was not fun, and I decided to shelve it until new ideas present themselves. Here I will present a list of the problems, the fixes I attempted, and further explanations of why they didn't work.

<h3>Problem 1</h3>

Problem 1: There's no real way for the player to predict the trajectory of the ball after the first collision, hence the player will never be able to establish a relationship between their input and the outcomes of their choices. When a player begins a game, they must first establish the rules that govern the game, then iteratively take new approaches to completing the game's challenge, and the player must have "faith" that the game's rules will be obeyed consistently. If the player cannot ascertain the relationships between systems, it stops being a game and becomes a toy or a technical demo.

<h4>Fix 1</h4>

I created a pair of meshes whose texture popped out of the cannon and tweened forward towards the targeted object. And whatever object the mesh "collided" with, the second mesh would be oriented in the trajectory of the bounce. This was to help the player understand the physical rules governing the game and let the player know how the first collision will work. It was a nice idea, and an especially helpful debugging tool, but sadly one collision does not mean much when the next four or five could result in a quick game over.

<h4>Fix 2</h4>

Second fix to Problem 1: add an AI to the superball that caused it to change its direction the moment it exited a collision (if it was pointed at an unbreakable object). It would try its hardest to redirect itself to a trajectory that would hit a breakable object, keeping up the momentum of crashes, increasing the ball's duration-of-flight. The ball has a field of view of n-degrees (shaped like a cone), and its range of movement was a cone of m degrees (where m < n).

This fix was an interesting challenge to implement, and the results were decent in a few cases: the ball could orient itself to hit a bunch of plates, and was smart enough to align itself to destroy entire rows of plates at a time. When it doesn't work, though, it's even more unpredictable. In cases where the nearest breakable object is outside the ball's range of movement, the ball will change its trajectory to so it collides with an object as close to the breakable object as possible, which may cause the ball to move in directions that move the ball further away from breakable objects. The robustness of the AI could be improved in future iterations.

<h3>Problem 2</h3>

Players' interaction with the game ends once they shoot the ball. As a result, the game was 5% player decision and 95% watch things break, which in most cases is not terribly fun. There might be ways to fix this, and I have experimented with a few:

<h4>First Fix</h4>

Designate some breakable objects with a "Re-aiming" trait. When the ball collides with a breakable object assigned the Re-aiming script, the ball will freeze-in-place, and the player earns the chance to reaim the ball from the breakable object's position. With this, the player has more chances to interact with the game in that 95% of non-playtime. My biggest concern was that since players could not predict the ball's trajectory beyond 2 collisions, the frequency this mechanic will become available in each level would be random.

<h4>Second Fix</h4>

There were several other fixes I intended to implement to Problem 2. The most interesting one was an item called a "gravity well", that the player would put on the map before firing the cannon. A gravity well would be placed in the level, and if the ball's trajectory brought it close enough to the gravity well, it would pull the ball into its "black hole", then spit it out in the direction specified by the player at the beginning of the game.

This would have added an interesting dynamic to the game: the player would have a limited number of gravity wells, and to get the maximum usage out of them (i.e. increase the number of broken objects) the player would have to anticipate the sections of the level where the ball would not reach, based on where the player placed and aimed his cannon. And that's where the idea falls apart: as described in Problem 1, the player cannot predict the outcome no matter what position or in what orientation the cannon is. Without a reliable system to plan against, the player is unable to "strategize" the placement of gravity wells.
