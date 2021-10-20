SELECT p.SecondName, p.FirstName, p.Patronymic, p.DateBirth, Pol.PolisNumber, test.DateTest,
		place.NamePlace, place.Address, place.NumberPhone, place.Email, ResultTest, res.Accuracy, type.Name FROM Patient p
		JOIN Polis Pol ON p.IdPolis = Pol.Id
		JOIN Test test ON test.IdPatient = p.Id
		JOIN Place place ON test.IdPlace = place.Id
		JOIN Result res ON test.IdResult = res.Id
		JOIN TypeTest type ON test.IdTypeTest = type.Id
		WHERE 'Bond' = p.SecondName AND 'Eugene' = p.FirstName 
				AND 'Hamish' = p.Patronymic AND '3805355829247019' = Pol.PolisNumber; ;


		