CREATE DATABASE acceloka_db;

CREATE TABLE "Categories" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL
);

CREATE TABLE "Tickets" (
    "Id" UUID PRIMARY KEY,
    "CategoryId" UUID NOT NULL,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(200) NOT NULL,
    "EventDate" TIMESTAMP NOT NULL,
    "Price" NUMERIC(18,2) NOT NULL,
    "Quota" INT NOT NULL,
    CONSTRAINT "FK_Tickets_Categories"
        FOREIGN KEY ("CategoryId")
        REFERENCES "Categories"("Id"),
    CONSTRAINT "CHK_Ticket_Quota"
        CHECK ("Quota" >= 0)
);

CREATE TABLE "BookedTickets" (
    "Id" UUID PRIMARY KEY,
    "BookingDate" TIMESTAMP NOT NULL
);

CREATE TABLE "BookedTicketDetails" (
    "Id" UUID PRIMARY KEY,
    "BookedTicketId" UUID NOT NULL,
    "TicketId" UUID NOT NULL,
    "Quantity" INT NOT NULL,
    CONSTRAINT "FK_Detail_Booked"
        FOREIGN KEY ("BookedTicketId")
        REFERENCES "BookedTickets"("Id")
        ON DELETE CASCADE,
    CONSTRAINT "FK_Detail_Ticket"
        FOREIGN KEY ("TicketId")
        REFERENCES "Tickets"("Id"),
    CONSTRAINT "CHK_Quantity"
        CHECK ("Quantity" > 0)
);

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

INSERT INTO "Categories" ("Id","Name") VALUES
('11111111-1111-1111-1111-111111111111','Transportasi Darat'),
('22222222-2222-2222-2222-222222222222','Transportasi Laut'),
('33333333-3333-3333-3333-333333333333','Cinema'),
('44444444-4444-4444-4444-444444444444','Hotel'),
('55555555-5555-5555-5555-555555555555','Concert');

INSERT INTO "Tickets"
("Id","CategoryId","Code","Name","EventDate","Price","Quota")
VALUES
(gen_random_uuid(),'11111111-1111-1111-1111-111111111111','TD001','Bus Jawa-Sumatra','2026-03-02 17:59',500000,80),
(gen_random_uuid(),'11111111-1111-1111-1111-111111111111','TD002','Bus Jakarta-Bali','2026-03-10 10:00',450000,70),
(gen_random_uuid(),'22222222-2222-2222-2222-222222222222','TL001','Kapal Ferry Jawa-Sumatra','2026-03-05 08:00',600000,50),
(gen_random_uuid(),'33333333-3333-3333-3333-333333333333','C001','Avengers CGV','2026-03-02 19:00',75000,99),
(gen_random_uuid(),'33333333-3333-3333-3333-333333333333','C002','Ironman CGV','2026-03-04 21:00',70000,60),
(gen_random_uuid(),'44444444-4444-4444-4444-444444444444','H001','Ibis Jakarta 21-23','2026-03-02 14:00',850000,40),
(gen_random_uuid(),'44444444-4444-4444-4444-444444444444','H002','Hilton Bandung 3D2N','2026-03-15 14:00',1500000,30),
(gen_random_uuid(),'55555555-5555-5555-5555-555555555555','CT001','Coldplay Concert','2026-04-01 20:00',2000000,100),
(gen_random_uuid(),'55555555-5555-5555-5555-555555555555','CT002','Bruno Mars Concert','2026-04-10 20:00',1800000,75),
(gen_random_uuid(),'22222222-2222-2222-2222-222222222222','TL002','Kapal Bali-Lombok','2026-03-12 09:00',550000,65);


