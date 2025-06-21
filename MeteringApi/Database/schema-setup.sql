CREATE TABLE metering.account (
	id serial NOT NULL,
	first_name varchar NOT NULL,
	last_name varchar NOT NULL,
	CONSTRAINT account_pk PRIMARY KEY (id)
);

CREATE TABLE metering.meter_reading (
	account_id integer NOT NULL,
	reading_datetime timestamp NOT NULL,
	read_value integer NOT NULL,
	CONSTRAINT meter_reading_pk PRIMARY KEY (account_id,reading_datetime),
	CONSTRAINT meter_reading_account_fk FOREIGN KEY (account_id) REFERENCES metering.account(id)
);