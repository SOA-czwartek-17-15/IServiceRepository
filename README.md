#IServiceRepository
## Krótki opis projektu

IServiceRepository to interfejs, dzięki któremu wszystkie serwisy mogą pobierać dane o lokalizacji innych serwisów.
W tym celu rejestrują się korzystając z metody RegisterService. Następnie wywołują metodę Alive, która potwierdza że serwis nie padł i są nadal obecni.
Po 10s od ostatniego kontaktu serwis zostaje uznany za martwy i jest usuwany z bazy. Teoretycznie serwisy powinny się wyrejestrowywać przy użyciu Unregister natomiast i tak zostaną po jakimś czasie usunięte. 
Za pomocą metody GetServiceLocation oraz GetServiceLocations pobierają natomiast informacje o pozostałych serwisach.
