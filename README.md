# Superball-Destruction

This was an attempt to make a set of game mechanics that turned a rubber superball into an object players could throw in a room filled with objects.

In this game, the player aims a cannon and fires it at ceramic plates in a small room. If the ball hits a breakable object its velocity will increase. If it hits an unbreakable object, its velocity decreases. Once the superball's velocity reaches a value less than 5mph, the ball's velocity becomes influenced by gravity. The game ends when the y-component of the ball's velocity is <= 0.0f. Your score is the number of breakable objects you hit x 100.

The aesthetic is simple: breaking imaginary things is fun! (real things? Not so much)

In terms of programming, it was a decent success. I created a shader and texture that displays the ball's trajectory when it's shot out of the cannon, I used Unity3D's physics system to rewrite the rules of physics for the superball, I was able to use an event messaging system (which made scripting much easier), and I learned how to use Quaternions.

(I'll discuss the shortcomings of the design before the end of the day)
