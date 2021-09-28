CREATE TABLE public.category (
    category_id uuid DEFAULT public.uuid_generate_v1mc() NOT NULL,
    name text NOT NULL
);
ALTER TABLE ONLY public.category
    ADD CONSTRAINT category_pkey PRIMARY KEY (category_id);
