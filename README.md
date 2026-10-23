# GD3_JeuDeLoie_CV_Unity

Goblin Adventure

Jeu type jeu de l'oie
On joue en 3ème personne, on joue un gobelin qui s'est fait enfermer dans un circuit par un génie après s'être moqué de lui. Le gobelin va devoir faire 5 tours en survivant à tous ses défis.

Idées de défis :
-Course d'obstacles
-Mini platformer
-Cache-cache
-Défis contre la montre

Le jeu doit mettre en place :
-Déplacement sur des cases avec des actions sur certaines cases. (= +2 cases, -3 cases, …)
-Actions de dialogues et / ou de quêtes. (interactions entre le gobelin et le génie)
-Actions déclenchant un mini jeu de cache-cache. (case d'un des défis)
-Actions bonus de collecte de ressources, achats d'objets par exemple. (à chaque fin de tour, le génie donne une vie supplémentaire, si le gobelin tombe exactement sur la case choisie au hasard par le génie, il pourra la récupérer et ainsi réessayer un défi si il meurt)


EXPLICATIONS CODE


Script Tile

Le script Tile représente une seule case du plateau. Il ne contient que des données, pas de logique de jeu.

Il a deux informations : un type, défini par un enum appelé TileType : une case est soit Normal, soit Bonus. 
moveAmount (un entier), qui indique de combien de cases on se déplace. S’il est positif, le joueur avance, par exemple +3. S’il est négatif, il recule, par exemple -2. Si la case est normale, cette valeur ne sert à rien.

Ce script est placé sur chaque case du prefab, et on modifier ses valeurs dans l’Inspector case par case. C’est ce qui me permet de créer facilement des bonus et des malus sans toucher au code.

J’ai aussi ajouté OnDrawGizmos qui dessine un petit cercle au-dessus de chaque case dans la vue Scene : blanc pour une case normale, vert pour un bonus, rouge pour un malus. Cela sert uniquement à me repérer dans l’éditeur, on ne le voit pas en jeu.


Script Board

Le script Board gère le circuit complet. Il a deux rôles : construire le plateau et fournir des outils aux autres scripts pour s’y repérer.

Premier rôle : la génération : on a une méthode Build() qu'on peux lancer depuis l’éditeur avec un clic droit sur le composant, via ContextMenu. Elle prend un prefab de case, un nombre de cases tileCount et un rayon radius. Pour chaque case, elle calcule un angle : on divise le cercle complet (soit 2π) par le nombre de cases. Avec le cosinus et le sinus de cet angle, multipliés par le rayon, on obtient la position de la case sur le cercle. 
La case est instanciée à cet endroit, nommée Case_00, Case_01, etc., et orientée vers le centre. Toutes les cases créées sont rangées dans une liste tiles, dans l’ordre du circuit.

Deuxième rôle : la navigation : comme le plateau est une boucle, après la dernière case on retourne à la première, on applique une méthode Wrap. Elle utilise un modulo, écrit de manière à toujours donner un résultat positif. Avancer depuis la dernière case donne la case 0, et reculer depuis la case 0 donne la dernière case. Ça marche donc dans les deux sens.

Enfin, GetTile(index) renvoie la case correspondant à un numéro, en appliquant automatiquement Wrap. Les autres scripts n’ont donc jamais à se soucier des débordements.


Script PlayerMover

Le script PlayerMover s’occupe uniquement du déplacement du pion. Il ne décide jamais de combien on avance, il exécute ce qu’on lui demande.

Il garde en mémoire une variable CurrentIndex, qui est le numéro de la case sur laquelle le pion se trouve. Au démarrage il se place sur la case 0 avec un petit décalage en hauteur pour ne pas être enfoncé dans la case.

Son cœur, c’est la méthode MoveSteps(steps), qui est une coroutine. Une coroutine permet de faire une action étalée dans le temps : ici, le pion avance case par case, de façon visible, au lieu de se téléporter. Si steps est positif, la direction est +1, s’il est négatif, c’est -1, donc le recul fonctionne avec la même méthode.

À chaque pas, le script met à jour CurrentIndex en passant par board.Wrap, récupère la position de la case suivante, et appelle une deuxième coroutine, MoveTo. Celle-ci tourne le pion vers sa destination, puis le fait glisser avec Vector3.MoveTowards à une vitesse réglable, jusqu’à l’arrivée. Il y a une courte pause entre chaque case pour que le mouvement soit lisible.

Il expose aussi IsMoving, qui indique si le pion est en train de bouger.


Script GameManager

Le GameManager est le script central, c’est lui qui fait fonctionner la partie et qui coordonne tous les autres.

Il connaît le Board, le PlayerMover, le bouton de lancer, le texte d’information, l’image du dé et ses 6 sprites.

Au démarrage, dans Start, il branche le bouton sur la méthode OnRollClicked, affiche le message de départ et met la face 1 du dé.

Quand on clique sur le bouton, on lance la coroutine TurnRoutine, qui déroule un tour complet, étape par étape :

Elle bloque le bouton et met un booléen isBusy à vrai, pour empêcher de relancer le dé pendant un tour.
Elle tire le résultat avec Random.Range(1, 7). Le deuxième chiffre est exclu pour les entiers, donc on obtient bien de 1 à 6.
Elle lance l’animation du dé, DiceRollAnimation. Pendant environ une seconde les faces changent au hasard, le dé tourne sur lui-même et rebondit en s’amortissant. À la fin, il s’arrête sur la face correspondant au résultat : le sprite utilisé est diceFaces[result - 1], car le tableau commence à 0. Le résultat est donc décidé avant l’animation, qui n’est que visuelle.
Elle demande au PlayerMover d’avancer du nombre obtenu, et attend la fin du déplacement.
Elle appelle ResolveTile, qui regarde la case d’arrivée grâce au Board. Si c’est un bonus ou un malus, elle affiche un message, attend une seconde puis redéplace le pion de moveAmount cases. Comme on peut tomber sur une autre case bonus, c’est une boucle while. J’ai ajouté un compteur de sécurité limité à 10, pour éviter une boucle infinie si deux cases se renvoient l’une vers l’autre. À la fin, le message « Relance le dé ! » s’affiche.
Enfin, elle réactive le bouton et remet isBusy à faux.
