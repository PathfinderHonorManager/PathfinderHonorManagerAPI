CREATE TABLE public.pathfinder_class (
    grade integer NOT NULL,
    name text NOT NULL
);
ALTER TABLE ONLY public.pathfinder_class
    ADD CONSTRAINT pathfinder_class_pkey PRIMARY KEY (grade);