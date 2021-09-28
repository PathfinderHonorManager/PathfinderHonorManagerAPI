CREATE TABLE public.honor (
    honor_id uuid DEFAULT public.uuid_generate_v1mc() NOT NULL,
    name text NOT NULL,
    level integer NOT NULL,
    description text,
    patch_path text NOT NULL,
    wiki_path text,
    category_id uuid,
    patch_image_page_path text
);
ALTER TABLE ONLY public.honor
    ADD CONSTRAINT honor_pkey PRIMARY KEY (honor_id);
ALTER TABLE ONLY public.honor
    ADD CONSTRAINT category_id_fkey FOREIGN KEY (category_id) REFERENCES public.category(category_id);

