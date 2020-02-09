# :vertical_traffic_light: SuperDuperKartingGame :car:
*Un projet de Amir BEN HAJ KHALED et Michel CHAKER .*

### SuperDuperKartingGame c'est quoi?

SuperDuperKartingGame est un jeu PC de Karting qui ce joue avec une webcam et deux marquers **magenta** à gauche et **vert** à doite collés sur les côtés d'un bout de carton qui fait office de volant.
(les post-it ont l'avantage d'être de la bonne taille et d'être autocollants, sinon utilisez ceux fournis dans ce *README* à découper).

![Papiers](/Images/Papiers.jpg)


### Comment jouer au jeu?

Télecharger la release qui se trouve dans [ce repo](/SuperDuperKartingGame), ou lancer le projet avec *Unity 2018.4*.

Le menu "calibrate" vous permet de choisir la valeur au-delà de laquelle vous avencez et celle pour freiner et reculer.
![Accélérer](/Images/Accelerate.png)
![Freiner](/Images/Brake.png)
![Valider](/Images/Validate.png)


### Comment ca marche?

On utilise [Emgu CV](http://www.emgu.com/wiki/index.php/Main_Page) pour bénéficier de **opencv** avec unity. Cela nous permet de récupérer le flux vidéo de la webcam, pour détecter les marqueurs **magenta** et **vert** et ainsi calculer la rotation du "volant".

À chaque Frame capturer par la webcam: 
- On convertit l'image dans l'espace de couleurs **HSV**.
- On applique un flou Gaussien.
- à partir de cette nouvelle image on crée deux (une pour chaque marqueur) que l'on transforme en image binaire (noir et blanc) à partir   des seuils relative au **magenta** et au **vert**. 
Sur les deux images relative aux marqueurs:
- On applique une érosion et une dilatation pour améliorer le résultat.
- On détecte le contour de la plus grande forme et on extrait le contour minimal qui encapsule la forme trouver.
Puis:
- On détecte la rotation en dergré à partir des centres des deux contours trouvés.
- On calcule le contour qui englobe les deux marquers et on additionne la hauteur et la largeur de ce contour pour savoir la position du volant par rapport à la webcam.
- Pour avoir de meilleur résultat on calcule la moyenne de 5 valeurs, enregistrées dans un buffer, pour ensuite calculer la rotation et largeur du volant.

![Détection](/Images/Detection.jpg)

### Vidéo

[![Vidéo](https://img.youtube.com/vi/VHNzlEDG07Y/0.jpg)](https://youtu.be/VHNzlEDG07Y)
