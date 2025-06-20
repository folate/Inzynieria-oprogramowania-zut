# Inzynieria-oprogramowania-zut

Ten projekt to prototyp na przedmiot Inzynieria oprogramowania

## Wymagania wstępne

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Node.js](https://nodejs.org/)
- [npm](https://www.npmjs.com/) 
- [PostgreSQL](https://www.postgresql.org/) 

## Backend (ASP.NET Core)

1. **Przejdź do katalogu backendu:**
   ```sh
   cd TaixRideAPI
   ```
2. **Przywróć zależności:**
   ```sh
   dotnet restore
   ```
3. **Zastosuj migracje bazy danych:**
   ```sh
   dotnet ef database update
   ```
   > Upewnij się, że connection string w pliku `Program.cs` jest poprawny dla Twojej bazy danych.
4. **Uruchom backend:**
   ```sh
   dotnet run
   ```
   Backend domyślnie uruchomi się na `http://localhost:5145`.


## Frontend (Svelte)

1. **Przejdź do katalogu frontendu:**
   ```sh
   cd taxiride-frontend
   ```
2. **Zainstaluj zależności:**
   ```sh
   npm install
   ```
3. **Uruchom frontend:**
   ```sh
   npm run dev
   ```
   Frontend domyślnie uruchomi się na `http://localhost:5173`.


## Użytkowanie
- baza jest wypełniona przykładowymi danymi przy uruchomieniu (jeżeli tabele są puste)
- przykładowy użytkownik login: john.doe hasło: password123
- przykładowy właściciel login: owner1 hasło: password123
- przykładowy kierowca login: driver1 hasło: password123
- przykładowy pracownik login: worker1 hasło: password123
