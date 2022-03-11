using System;
using System.Collections;
using UnityEngine;

namespace AngryDash.Game
{
    public class Mur : MonoBehaviour
    {
        private Player player;
        public float colider;
        public float boostMultiplier;
        public float blockID = 1;

        public Vector2 Move;

        private bool Colliding;

        private void Update()
        {
            if (Move != new Vector2() & Colliding & player != null)
            {
                var playerMove = new Vector2();
                if (player.transform.position.x > transform.position.x & Move.x > 0) playerMove.x = Move.x * 50;
                else if (player.transform.position.x < transform.position.x & Move.x < 0) playerMove.x = Move.x * 50;
                if (player.transform.position.y > transform.position.y & Move.y > 0) playerMove.y = Move.y * 50;
                else if (player.transform.position.y < transform.position.y & Move.y < 0) playerMove.y = Move.y * 50;
                player.transform.Translate(playerMove, Space.World);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider) { Collision(collider.gameObject); }
        private void OnCollisionEnter2D(Collision2D collision) { if ((int)colider == 3) Collision(collision.gameObject, collision); }

        private void Collision(GameObject Player, Collision2D collision = null)
        {
            Colliding = true;
            player = Player.GetComponent<Player>();

            if ((int)colider == 0) //Stop
            {
                player.PeutAvancer = false;
                player.vitesse = 0;
                StartCoroutine(colid(0.5F));
            }
            else if ((int)colider == 1) //Kill
            {
                player.PeutAvancer = false;
                player.vitesse = 0;
                player.transform.position = player.PositionInitiale;
                player.onRespawn?.Invoke();
                StartCoroutine(colid(0.1F));
            }
            else if (colider >= 2.1F & colider < 3) //No Collision + Boost
                player.levelSettings.distance += boostMultiplier;
            else if ((int)colider == 3) //Bounce
            {
                if (!player.Touched & collision != null)
                {
                    Vector2 playerPos = player.transform.TransformDirection(Vector2.up);
                    var contact = collision.GetContact(0);
                    var reflect = Vector2.Reflect(playerPos, contact.normal);
                    player.transform.rotation = Quaternion.FromToRotation(Vector2.up, reflect);

                    if (colider >= 3.1F & boostMultiplier > 0) player.levelSettings.distance += boostMultiplier;
                    player.Touched = true;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision) { if ((int)colider != 3) CollisionExit(); }
        private void OnCollisionExit2D(Collision2D collision) { if ((int)colider == 3) CollisionExit(); }

        private void CollisionExit()
        {
            Colliding = false;
            player.Touched = false;
        }

        private void Start()
        {
            if ((int)colider == 0 | (int)colider == 3)
                GetComponents<Collider2D>()[1].enabled = true;
            else GetComponents<Collider2D>()[1].enabled = false;

            if (colider != (int)colider)
                boostMultiplier = int.Parse(colider.ToString().Split(new string[1] { "." }, StringSplitOptions.None)[1]);
            else boostMultiplier = 0;
        }

        private IEnumerator colid(float wait)
        {
            yield return new WaitForSeconds(wait);
            player.PeutAvancer = true;
            player.vitesse = 1;
        }
    }
}