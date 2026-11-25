import express from 'express';

const app = express();
app.use(express.json());

const events = [
    { id: 1, name: 'Футбол', category: 'Спорт', date: '2027-05-01',
      location: 'Стадион', capacity: 1, registrations: [] },

    { id: 2, name: 'Волейбол', category: 'Спорт', date: '2027-05-02',
      location: 'Спортзал', capacity: 12, registrations: [] },

    { id: 3, name: 'Забег', category: 'Спорт', date: '2027-05-03',
      location: 'Парк', capacity: 30, registrations: [] },

    { id: 4, name: 'Основы Node.js', category: 'Обучение', date: '2027-05-04',
      location: 'Аудитория', capacity: 15, registrations: [] },

    { id: 5, name: 'Урок фотографии', category: 'Обучение', date: '2027-05-05',
      location: 'Студия', capacity: 10, registrations: [] },

    { id: 6, name: 'Концерт', category: 'Культура', date: '2027-05-06',
      location: 'Концертный зал', capacity: 50, registrations: [] },

    { id: 7, name: 'Выставка', category: 'Культура', date: '2027-05-07',
      location: 'Галерея', capacity: 25, registrations: [] },

    { id: 8, name: 'Кинопоказ', category: 'Культура', date: '2027-05-08',
      location: 'Кинотеатр', capacity: 40, registrations: [] }
];

function findEvent(id) {
    return events.find(event => event.id === Number(id));
}

function freeSeats(event) {
    return event.capacity - event.registrations.length;
}

function validEvent({ name, category, date, location, capacity }) {
    if (typeof name !== 'string' || !name.trim() ||
        typeof category !== 'string' || !category.trim() ||
        typeof location !== 'string' || !location.trim() ||
        !Number.isSafeInteger(capacity) || capacity < 1) {
        return false;
    }

    if (typeof date !== 'string' || date.length !== 10) {
        return false;
    }

    const day = new Date(date + 'T00:00:00Z');
    return !Number.isNaN(day.getTime()) &&
        day.toISOString().slice(0, 10) === date;
}

app.get('/events', (req, res) => {
    const { category, available } = req.query;
    let result = events;

    if (category) {
        result = result.filter(event => event.category === category);
    }

    if (available === 'true') {
        result = result.filter(event => freeSeats(event) > 0);
    }

    res.json(result);
});

app.get('/events/:id', (req, res) => {
    const event = findEvent(req.params.id);

    if (!event) {
        return res.status(404).json({ error: 'Мероприятие не найдено' });
    }

    res.json(event);
});

app.post('/events', (req, res) => {
    const fields = req.body ?? {};

    if (!validEvent(fields)) {
        return res.status(400).json({ error: 'Некорректные данные мероприятия' });
    }

    const { name, category, date, location, capacity } = fields;
    const newEvent = {
        id: Math.max(0, ...events.map(event => event.id)) + 1,
        name: name.trim(),
        category: category.trim(),
        date,
        location: location.trim(),
        capacity,
        registrations: []
    };

    events.push(newEvent);
    res.status(201).json(newEvent);
});

app.put('/events/:id', (req, res) => {
    const event = findEvent(req.params.id);

    if (!event) {
        return res.status(404).json({ error: 'Мероприятие не найдено' });
    }

    const fields = req.body ?? {};
    if (!validEvent(fields)) {
        return res.status(400).json({ error: 'неврные данные мероприятия' });
    }

    const { name, category, date, location, capacity } = fields;
    if (capacity < event.registrations.length) {
        return res.status(409).json({ error: 'Вместимость меньше числа участников' });
    }

    event.name = name.trim();
    event.category = category.trim();
    event.date = date;
    event.location = location.trim();
    event.capacity = capacity;

    res.json(event);
});

app.get('/stats', (req, res) => {
    let totalRegistrations = 0;
    let eventsWithAvailableSeats = 0;
    let mostPopularEvent = null;

    for (const event of events) {
        const count = event.registrations.length;
        totalRegistrations += count;

        if (freeSeats(event) > 0) {
            eventsWithAvailableSeats++;
        }

        if (!mostPopularEvent || count > mostPopularEvent.registrations.length) {
            mostPopularEvent = event;
        }
    }

    res.json({
        totalEvents: events.length,
        totalRegistrations,
        eventsWithAvailableSeats,
        mostPopularEvent
    });
});
app.delete('/events/:id', (req, res) => {
    const id = Number(req.params.id);
    const event = events.find(e => e.id === id);

    if (!event) {
        return res.status(404).json({ error: 'Мероприятие не найдено' });
    }

    if (event.registrations.length > 0) {
        return res.status(409).json({
            error: 'Нельзя удалить мероприятие с участниками'
        });
    }

    events.splice(events.indexOf(event), 1);
    res.json({ message: 'Мероприятие удалено' });
});
let nextRegistrationId = 1;

app.get('/events/:id/registrations', (req, res) => {
    const event = findEvent(req.params.id);
    if (!event) {
        return res.status(404).json({ error: 'Мероприятие не найдено' });
    }

    res.json(event.registrations);
});

app.post('/events/:id/registrations', (req, res) => {
    const event = findEvent(req.params.id);
    if (!event) {
        return res.status(404).json({ error: 'Мероприятие не найдено' });
    }

    const body = req.body ?? {};
    const name = typeof body.name === 'string' ? body.name.trim() : '';
    const email = typeof body.email === 'string'
        ? body.email.trim().toLowerCase()
        : '';

    if (!name || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        return res.status(400).json({ error: 'Укажи имя и корректный email' });
    }

    if (event.registrations.some(r => r.email === email)) {
        return res.status(409).json({ error: 'Этот email уже зарегистрирован' });
    }

    if (freeSeats(event) <= 0) {
        return res.status(409).json({ error: 'Свободных мест нет' });
    }

    const registration = { id: nextRegistrationId++, name, email };
    event.registrations.push(registration);
    res.status(201).json(registration);
});

app.delete('/events/:id/registrations/:registrationId', (req, res) => {
    const event = findEvent(req.params.id);
    if (!event) {
        return res.status(404).json({ error: 'Мероприятие не найдено' });
    }

    const id = Number(req.params.registrationId);
    const index = event.registrations.findIndex(r => r.id === id);
    if (index === -1) {
        return res.status(404).json({ error: 'Регистрация не найдена' });
    }

    event.registrations.splice(index, 1);
    res.json({ message: 'Регистрация удалена' });
});
app.listen(3000, () => {
    console.log('Сервер запущен');
});