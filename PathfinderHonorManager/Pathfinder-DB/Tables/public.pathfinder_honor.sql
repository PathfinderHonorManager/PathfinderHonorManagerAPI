CREATE TABLE public.pathfinder_honor (
    pathfinder_honor_id uuid DEFAULT public.uuid_generate_v1mc() NOT NULL,
    pathfinder_id uuid NOT NULL,
    honor_id uuid NOT NULL,
    status_code integer NOT NULL,
    create_timestamp timestamp with time zone NOT NULL
);
ALTER TABLE ONLY public.pathfinder_honor
    ADD CONSTRAINT pathfinder_honor_pkey PRIMARY KEY (pathfinder_honor_id);
CREATE INDEX fki_honor_id_fkey ON public.pathfinder_honor USING btree (honor_id);
CREATE INDEX fki_pathfinder_id_fkey ON public.pathfinder_honor USING btree (pathfinder_id);
CREATE INDEX fki_status_code_fkey ON public.pathfinder_honor USING btree (status_code);
ALTER TABLE ONLY public.pathfinder_honor
    ADD CONSTRAINT honor_id_fkey FOREIGN KEY (honor_id) REFERENCES public.honor(honor_id);
ALTER TABLE ONLY public.pathfinder_honor
    ADD CONSTRAINT pathfinder_id_fkey FOREIGN KEY (pathfinder_id) REFERENCES public.pathfinder(pathfinder_id);
ALTER TABLE ONLY public.pathfinder_honor
    ADD CONSTRAINT status_code_fkey FOREIGN KEY (status_code) REFERENCES public.pathfinder_honor_status(status_code);
CREATE TRIGGER create_timestamp_trigger BEFORE INSERT ON public.pathfinder_honor FOR EACH ROW EXECUTE FUNCTION public.trigger_set_created_timestamp();

