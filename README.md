# ZADATAK 15
Kreirati Web server koji klijentu omogućava prikaz vremenske prognoze uz pomoć WeatherAPI-
a. Pretraga se može vršiti pomoću filtera koji se definišu u okviru query-a. Vremenska prognoza,
kao i podaci o kvalitetu vazduha se vraćaju kao odgovor klijentu (podaci o kvalitetu vazduha su
dostupni korišćenjem aqi parametra). Svi zahtevi serveru se šalju preko browser-a korišćenjem
GET metode. Ukoliko navedena prognoza ne postoji, prikazati grešku klijentu.
Primer poziva serveru:
http://api.weatherapi.com/v1/current.json?key=&q=London&aqi=no
Način funkcionisanja WeatherAPI-a je moguće proučiti na sledećem linku:
https://www.weatherapi.com/docs/
