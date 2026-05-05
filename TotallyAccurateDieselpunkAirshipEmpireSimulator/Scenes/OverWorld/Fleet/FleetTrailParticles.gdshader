shader_type particles;


void vertex(){
	UV.x = (INSTANCE_CUSTOM.w-1.0 - UV.x)/INSTANCE_CUSTOM.z;
}

void start() {
	// Called when a particle is spawned.
}

void process() {
	// Called every frame on existing particles (according to the Fixed FPS property).
}
