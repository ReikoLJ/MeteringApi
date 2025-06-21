-- Add schema changes here that are required during development.

CREATE TABLE metering.account (
	id integer NOT NULL,
	first_name varchar NOT NULL,
	last_name varchar NOT NULL,
	CONSTRAINT account_pk PRIMARY KEY (id)
);


CREATE TABLE metering.meter_reading (
	id integer NOT NULL,
	account_id integer NOT NULL,
	reading_datetime date NOT NULL,
	read_value integer NOT NULL,
	CONSTRAINT meter_reading_pk PRIMARY KEY (id),
	CONSTRAINT meter_reading_account_fk FOREIGN KEY (account_id) REFERENCES metering.account(id)
);