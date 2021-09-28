CREATE TABLE public.pathfinder_honor_status (
    status_code integer NOT NULL,
    name text NOT NULL
);
ALTER TABLE ONLY public.pathfinder_honor_status
    ADD CONSTRAINT pathfinder_honor_status_pkey PRIMARY KEY (status_code);
ALTER TABLE ONLY public.pathfinder_honor_status
    ADD CONSTRAINT status_code_unique UNIQUE (status_code);
