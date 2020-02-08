# ComputerGraphics3D
Simple editor for 3D shapes


Instrukcja obsługi:

W mojej aplikacji bazowałem na lewoskrętnym układzie współrzędnych,
 czyli przy translacjach i pozycjach kamery duże Z to "daleko", a małe "blisko" (przynajmniej względem początkowego punkty widzenia kamery)

Sterowanie kamerą:
UWAGA: Przed manipulacją kamery polecam kliknąć na kamerę w liście obiektów, ponieważ jeżeli jakiś textbox będzie miał na sobie fokus to zaczniemy do niego pisać

W - ruch kamery w przód
S - ruch kamery w tył
A - obrót kamery w lewo
D - obrót kamery w prawo

8 - ruch kamery w górę
2 - ruch kamery w dół
6 - ruch kamery wzdłuż osi X                     
4 - ruch kamery w przeciwną stronę osi X  - powinno być w prawo i w lewo niezależnie od obecnego kierunku patrzenia kamery, to jest do poprawy



Przed stworzeniem Prostopadłościanu/Sfery/Kamery można wybrać początkowe parametry obok przycisku
	czyli wymiary/promień i liczbę podziałów/pozycję kamery (poczatkowo kamera będzie patrzyła wzdłuż osi Z)

Po kliknięciu na obiekt w liście obiektów, jego parametry translacji/skalowania/rotacji (lub pola widzenia i płaszczyzn obcinania w przypadku kamery)
wyświetlą się w odpowiedniej sekcji GUI i ich zmiana spowoduje zmianę na scenie.


Przykładowa scena jest zapisana w folderze bin/Debug w pliku Scene.xml
Zapisanie sceny nadpisuje plik Scene.xml w bin/Debug

Planuję w przyszłości dodać Z-Buffor i oświetlenie Phonga
