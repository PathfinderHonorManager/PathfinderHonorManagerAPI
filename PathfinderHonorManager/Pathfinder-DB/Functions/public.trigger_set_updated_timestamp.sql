CREATE FUNCTION public.trigger_set_updated_timestamp() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
  NEW.update_timestamp = NOW();
  RETURN NEW;
END;
$$;